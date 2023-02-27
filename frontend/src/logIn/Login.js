import { useState } from "react"
import styles from './LogIn.module.scss'
function LogIn() {
    const [logIn, setLogin] = useState(false);
    return (
        <div className="clear">
            <div className={styles.wrapper}>
                <div className={styles.register} >
                    <div className={styles.inputs}>
                        <h4>Log In</h4>
                        <ul>
                            <li className="mt-20">
                                <input className={styles.input} placeholder="Username" />
                            </li>
                            <li className="mt-20">
                                <input className={styles.input} placeholder="password" type='password' />
                            </li>
                            <li className="mt-40">
                                <button className={styles.button}>Submit</button>
                            </li>
                            <li>
                                <p className={styles.link}>Didn't have an account?</p>
                            </li>
                        </ul>
                    </div>
                    <img width={50} height={50} className={styles.img} src='/img/logInCat.png' alt='picture' />
                </div>
            </div>
        </div>
    )
}

export default LogIn;