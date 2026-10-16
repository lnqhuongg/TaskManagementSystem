import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { login } from '../services/auth.js'

export default function AuthPage({ mode, onLogin }) {
    const navigate = useNavigate()
    const isSignup = mode === 'register'
    const [loginError, setLoginError] = useState('')

    async function handleSubmit(event) {
        event.preventDefault()

        if (isSignup) {
            navigate('/user/tasks')
            return
        }

        const form = new FormData(event.currentTarget)

        try {
            const data = await login(form.get('email').trim(), form.get('password'))
            const user = onLogin(data)
            navigate(user.role === 'Admin' ? '/admin/tasks' : '/user/tasks')
        } catch (error) {
            const message = error instanceof ReferenceError
                ? 'Đã xảy ra lỗi khi đăng nhập.'
                : error.message || 'Đã xảy ra lỗi khi đăng nhập.'
            setLoginError(message)
        }
    }

    return (
        <main className="auth-screen">
            <Link className="auth-brand" to="/signin"><span className="brand-mark">t.</span><span>taskflow</span></Link>
            <section className="auth-panel">
                <div className="auth-kicker">WORK SMARTER, EVERY DAY</div>
                <h1>{isSignup ? 'Bắt đầu thật nhẹ nhàng.' : 'Chào mừng trở lại.'}</h1>
                <p>{isSignup ? 'Tạo tài khoản để sắp xếp công việc theo cách của bạn.' : 'Đăng nhập để tiếp tục với những công việc quan trọng.'}</p>
                <form onSubmit={handleSubmit} className="auth-form">
                    {isSignup && <label>Họ và tên<input name="name" required placeholder="Nguyễn Minh Anh" /></label>}
                    <label>Email<input name="email" type="email" required placeholder="example@email.com" /></label>
                    <label>Mật khẩu<input name="password" type="password" required minLength="6" placeholder="Tối thiểu 8 ký tự" /></label>
                    {isSignup && <label>Nhập lại mật khẩu<input type="password" name="confirmed_password" required placeholder="Xác nhận lại mật khẩu" /></label>}
                    {/* {!isSignup &&
                        <div className="auth-options"><label className="remember">
                            <input type="checkbox" /> Ghi nhớ đăng nhập</label>
                            <button className="btn btn-link" type="button" onClick={() => window.alert('Vui lòng liên hệ quản trị viên để đặt lại mật khẩu.')}>Quên mật khẩu?</button>
                        </div>
                    } */}
                    <button className="btn btn-primary button button-primary auth-submit" type="submit">{isSignup ? 'Tạo tài khoản' : 'Đăng nhập'} <span>→</span></button>
                </form>
                <div className="auth-switch">{isSignup ? 'Đã có tài khoản?' : 'Chưa có tài khoản?'} <Link to={isSignup ? '/signin' : '/register'}>{isSignup ? 'Đăng nhập' : 'Đăng ký miễn phí'}</Link></div>
            </section>
            {loginError && (
                <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) setLoginError('') }}>
                    <section className="dialog" role="alertdialog" aria-modal="true" aria-labelledby="login-error-title" aria-describedby="login-error-message">
                        <div className="dialog-header">
                            <div><div className="eyebrow">TASKFLOW WORKSPACE</div><h2 id="login-error-title">Không thể đăng nhập</h2></div>
                            <button className="btn icon-button" onClick={() => setLoginError('')} title="Đóng" aria-label="Đóng">×</button>
                        </div>
                        <p id="login-error-message">{loginError}</p>
                        <div className="dialog-actions">
                            <button className="btn btn-primary button button-primary" onClick={() => setLoginError('')}>Đã hiểu</button>
                        </div>
                    </section>
                </div>
            )}
        </main>
    )
}
