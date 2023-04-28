import './App.css';
import { Route, Routes } from 'react-router-dom'
import Login from './LogIn/Login'
import Registration from './Registration/Registration';
import Main from './MainPage/Main';
import PrivateRoute from './authContext';
function App() {
  return (
    <div className="App">
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/registration" exact element={<Registration />} />
        <PrivateRoute path="/main" exact element={<Main />} />
      </Routes>
    </div>
  );
}

export default App;
