﻿using HtmlAgilityPack;
using OpenQA.Selenium;
using System.Reflection.Metadata;
using Vacancies.Core.Consts;
using Vacancies.Core.Helpers;
using Vacancies.Core.Responses;

namespace Vacancies.Core.Services;

public interface IScrapperService
{
    ValueTask<List<VacancyResponse>> ScrapVacanciesByUrl(string path, IWebDriver driver);
}

public class ScrapperService : IScrapperService
{
    private bool isFinish = false;
    private readonly IActivateDriver _activateDriver;
    private readonly IElementFinder _elementFinder;

    public ScrapperService(
        IActivateDriver activateDriver,
        IElementFinder elementFinder)
    {
        _activateDriver = activateDriver ?? throw new ArgumentNullException(nameof(activateDriver));
        _elementFinder = elementFinder ?? throw new ArgumentNullException(nameof(elementFinder));
    }

    public async ValueTask<List<VacancyResponse>> ScrapVacanciesByUrl(string path, IWebDriver driver)
    {
        driver.Navigate().GoToUrl(path);

        await Task.Delay(200);

        return await ScrollVacancies(driver);
    }

    public async ValueTask<List<VacancyResponse>> ScrollVacancies(IWebDriver driver)
    {
        var paginationButton = _elementFinder.FindElementsByXpath(driver, ref XpathConsts.Xpath.PaginationButton);

        if (paginationButton is null)
            return await FillVacanciesAsync(driver);

        var items = await FillVacanciesAsync(driver);

        items = await FillWithPaginationAsync(driver, paginationButton);

        return items;
    }

    private async ValueTask<List<VacancyResponse>> FindVacancies(List<HtmlNode>? vacancies)
    {
        var vacancyResponse = new List<VacancyResponse>();
        string coordinates = "";
        var skills = new List<string>();
        string salary = null;

        foreach (var vacancy in vacancies)
        {
            var additionalDjinniPath = _elementFinder.FindSingleNodeByXpathAndElement(vacancy, ref XpathConsts.Xpath.DjinniCurrentVacancy, XpathConsts.ElementsToFind.AttributeToFind);

            if (additionalDjinniPath is not null)
            {
                var driver = await ActivateScrappingAsync(VacanciesConsts.DjinniUrl, additionalDjinniPath);

                var addtitionalDjiniInfoElements = driver.FindElements(By.XPath(XpathConsts.Xpath.DjinniAdditionalInfo))?
                    .FirstOrDefault();

                var skillsListElements = addtitionalDjiniInfoElements.FindElements(By.ClassName("job-additional-info--item")).ToList();

                foreach(var skillElement in skillsListElements)
                {
                    var skill = skillElement.FindElements(By.ClassName("job-additional-info--item-text")).FirstOrDefault().GetAttribute("innerText");
                    skills.Add(skill);
                }

                salary = driver.FindElements(By.XPath(XpathConsts.Xpath.DjinniAdditionalSalary))?.FirstOrDefault()?.GetAttribute("innerText");

                var douUri = _elementFinder.FindElementsByXpathAndAttribute(driver, ref XpathConsts.Xpath.DouCurrentUri, ref XpathConsts.ElementsToFind.AttributeToFind) ??
                             _elementFinder.FindElementsByXpathAndAttribute(driver, ref XpathConsts.Xpath.DouAdditionalCurrentUri, ref XpathConsts.ElementsToFind.AttributeToFind);

                if (douUri is null) { driver.Quit(); continue; }

                await GoToUriAsync(driver, douUri);

                var douDocument = _elementFinder.LoadHtmlByXpathAndAttribute(driver, ref XpathConsts.Xpath.DouNavBlockUri, ref XpathConsts.ElementsToFind.HtmlDocAttributeToFind);

                var douNavBlockElementsList = _elementFinder.FindChildNodesByElement(douDocument, XpathConsts.ElementsToFind.ListElementToFind);

                foreach (var navNode in douNavBlockElementsList)
                {
                    var innerTextOfElementInNavBlock = _elementFinder.FindSingleNodeInnerTextByXpath(navNode, ref XpathConsts.ElementsToFind.ElementInnerTextToFind);

                    if (innerTextOfElementInNavBlock is "Офіси")
                    {
                        var currentOfficeUri = _elementFinder.FindSingleNodeByXpathAndElement(navNode, ref XpathConsts.ElementsToFind.ElementInnerTextToFind, XpathConsts.ElementsToFind.AttributeToFind);

                        if (currentOfficeUri is null) { driver.Quit(); continue; }

                        await GoToUriAsync(driver, currentOfficeUri);

                        break;
                    }
                }

                var currentMapUri = _elementFinder.FindElementsByXpathAndAttribute(driver, ref XpathConsts.Xpath.DouMapDefaultUri, ref XpathConsts.ElementsToFind.AttributeToFind) ??
                                    _elementFinder.FindElementsByXpathAndAttribute(driver, ref XpathConsts.Xpath.DouMapAdditionalUri, ref XpathConsts.ElementsToFind.AttributeToFind);

                if (currentMapUri is null)
                {
                    driver.Quit();
                    continue;
                }

                await GoToUriAsync(driver, currentMapUri);

                var metaUri = _elementFinder.FindElementsByXpathAndAttribute(driver, ref XpathConsts.Xpath.DouMapMetaContent, ref XpathConsts.ElementsToFind.AttributeContentToFind) ??
                              _elementFinder.FindElementsByXpathAndAttribute(driver, ref XpathConsts.Xpath.DouMapAdditionalMetaContent, ref XpathConsts.ElementsToFind.AttributeContentToFind);

                coordinates = SplitUri(ref metaUri);

                driver.Quit();
            }

            var vacancyName = _elementFinder.FindSingleNodeInnerTextByXpath(vacancy, ref XpathConsts.Xpath.DjinniVacancyName) ??
                              _elementFinder.FindSingleNodeInnerTextByXpath(vacancy, ref XpathConsts.Xpath.DjinniAdditionalVacancyName);

            var response = new VacancyResponse(
                Guid.NewGuid(),
                vacancyName,
                skills,
                coordinates,
                salary);

            vacancyResponse.Add(response);
        }

        return vacancyResponse;
    }

    private List<HtmlNode?> ReloadDocument(IWebDriver driver)
    {
        var document = _elementFinder.LoadHtmlByXpathAndAttribute(driver, ref XpathConsts.Xpath.HtmlDoc, ref XpathConsts.ElementsToFind.HtmlDocAttributeToFind);

        var items = _elementFinder.FindChildNodesByElement(document, XpathConsts.ElementsToFind.ListElementToFind);

        return items;
    }

    private List<HtmlNode?> SetVacancies(IWebDriver driver)
    {
        var document = _elementFinder.LoadHtmlByXpathAndAttribute(driver, ref XpathConsts.Xpath.HtmlDoc, ref XpathConsts.ElementsToFind.HtmlDocAttributeToFind);

        return _elementFinder.FindChildNodesByElement(document, XpathConsts.ElementsToFind.ListElementToFind);
    }

    private async ValueTask<List<VacancyResponse>> FillVacanciesAsync(IWebDriver driver)
    {
        var vacancies = SetVacancies(driver);

        return await FindVacancies(vacancies);
    }

    private async ValueTask<List<VacancyResponse>> FillWithPaginationAsync(IWebDriver driver, IWebElement paginationButton)
    {
        var vacancies = new List<HtmlNode?>();
        var response = new List<VacancyResponse>();

        while (!isFinish)
        {
            if (paginationButton is not null)
            {
                paginationButton.Click();
                await Task.Delay(200);

                paginationButton = _elementFinder.FindElementsByXpath(driver, ref XpathConsts.Xpath.PaginationButton);

                var items = ReloadDocument(driver);

                if (items.Any())
                    foreach (var item in items)
                        vacancies.Add(item);

                response = await FindVacancies(vacancies);
            }
            else
                isFinish = true;
        }
        driver.Quit();

        return response;
    }

    private async ValueTask<IWebDriver?> ActivateScrappingAsync(string path, string additionalPath)
    {
        var driver = await _activateDriver.ActivateScrapingDriver();

        driver.Navigate().GoToUrl(path + additionalPath);

        await Task.Delay(200);

        return driver;
    }

    private async ValueTask GoToUriAsync(IWebDriver? driver, string uri)
    {
        driver?.Navigate().GoToUrl(uri);

        await Task.Delay(200);
    }

    private string SplitUri(ref string uri)
    {
        var firstPart = uri.Substring(uri.IndexOf("center=") + 7);

        return firstPart.Substring(0, firstPart.IndexOf("&zoom"));
    }
}
