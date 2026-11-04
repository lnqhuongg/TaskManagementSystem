import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { login, register } from '../services/auth.js'

export default function AuthPage({ mode, onLogin, onNotice }) {
    const navigate = useNavigate()
    const location = useLocation()
    const isSignup = mode === 'register'

    const [authError, setAuthError] = useState('')
    const [isSubmitting, setIsSubmitting] = useState(false)
    const registeredEmail = location.state?.registeredEmail || ''
    const successMessage = location.state?.successMessage || ''

    async function handleSubmit(event) {
        event.preventDefault()
        setAuthError('')
        const form = new FormData(event.currentTarget)

        if (isSignup) {
            const username = form.get('username')?.trim()
            const email = form.get('email')?.trim()
            const password = form.get('password')
            const confirmPassword = form.get('confirmPassword')

            if (password !== confirmPassword) {
                setAuthError('Mật khẩu và xác nhận mật khẩu không khớp.')
                return
            }

            setIsSubmitting(true)
            try {
                await register(username, email, password, confirmPassword)
                if (onNotice) {
                    onNotice('Đăng ký tài khoản thành công! Vui lòng đăng nhập.')
                }
                navigate('/signin', {
                    state: {
                        registeredEmail: email,
                        successMessage: 'Đăng ký tài khoản thành công! Vui lòng đăng nhập.'
                    }
                })
            } catch (error) {
                const message = error.message || 'Đã xảy ra lỗi khi đăng ký.'
                setAuthError(message)
            } finally {
                setIsSubmitting(false)
            }
            return
        }

        setIsSubmitting(true)
        try {
            const data = await login(form.get('email').trim(), form.get('password'))
            const user = onLogin(data)
            navigate(user.role === 'Admin' ? '/admin/tasks' : '/user/tasks')
        } catch (error) {
            const message = error instanceof ReferenceError
                ? 'Đã xảy ra lỗi khi đăng nhập.'
                : error.message || 'Đã xảy ra lỗi khi đăng nhập.'
            setAuthError(message)
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <main className="auth-screen">
            <Link className="auth-brand" to="/signin"><span className="brand-mark">t.</span><span>taskflow</span></Link>
            <section className="auth-panel">
                <div className="auth-kicker">WORK SMARTER, EVERY DAY</div>
                <h1>{isSignup ? 'Bắt đầu thật nhẹ nhàng.' : 'Chào mừng trở lại.'}</h1>
                <p>{isSignup ? 'Tạo tài khoản để sắp xếp công việc theo cách của bạn.' : 'Đăng nhập để tiếp tục với những công việc quan trọng.'}</p>

                {successMessage && !isSignup && (
                    <div style={{
                        padding: '10px 14px',
                        borderRadius: '8px',
                        background: 'rgba(34, 197, 94, 0.12)',
                        border: '1px solid rgba(34, 197, 94, 0.35)',
                        color: '#16a34a',
                        fontSize: '0.9rem',
                        marginBottom: '1rem',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '8px'
                    }}>
                        <span>✓</span> {successMessage}
                    </div>
                )}

                <form key={mode} onSubmit={handleSubmit} className="auth-form">
                    {isSignup && (
                        <label>
                            Tên đăng nhập
                            <input
                                name="username"
                                required
                                minLength="3"
                                maxLength="50"
                                pattern="[A-Za-z0-9_]+"
                                title="Chỉ dùng chữ cái không dấu, số và dấu gạch dưới"
                                placeholder="Ví dụ: minh_anh"
                                autoFocus
                            />
                        </label>
                    )}
                    <label>
                        Email
                        <input
                            name="email"
                            type="email"
                            required
                            maxLength="100"
                            defaultValue={!isSignup ? registeredEmail : ''}
                            placeholder="example@email.com"
                        />
                    </label>
                    <label>
                        Mật khẩu
                        <input
                            name="password"
                            type="password"
                            autoComplete={isSignup ? "new-password" : "current-password"}
                            required
                            minLength={isSignup ? 8 : 1}
                            maxLength="100"
                            pattern={isSignup ? "(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&#]).+" : undefined}
                            title={isSignup ? "Mật khẩu tối thiểu 8 ký tự, phải gồm ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt (@$!%*?&#)" : undefined}
                            placeholder={isSignup ? "Tối thiểu 8 ký tự (hoa, thường, số, ký tự đặc biệt)" : "Nhập mật khẩu"}
                        />
                    </label>
                    {isSignup && (
                        <label>
                            Nhập lại mật khẩu
                            <input
                                type="password"
                                name="confirmPassword"
                                autoComplete="new-password"
                                required
                                minLength="8"
                                maxLength="100"
                                placeholder="Xác nhận lại mật khẩu"
                            />
                        </label>
                    )}
                    <button
                        className="btn btn-primary button button-primary auth-submit"
                        type="submit"
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? (isSignup ? 'Đang tạo tài khoản...' : 'Đang đăng nhập...') : (isSignup ? 'Tạo tài khoản' : 'Đăng nhập')} <span>→</span>
                    </button>
                </form>
                <div className="auth-switch">{isSignup ? 'Đã có tài khoản?' : 'Chưa có tài khoản?'} <Link to={isSignup ? '/signin' : '/register'}>{isSignup ? 'Đăng nhập' : 'Đăng ký miễn phí'}</Link></div>
            </section>
            {authError && (
                <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) setAuthError('') }}>
                    <section className="dialog" role="alertdialog" aria-modal="true" aria-labelledby="auth-error-title" aria-describedby="auth-error-message">
                        <div className="dialog-header">
                            <div><div className="eyebrow">TASKFLOW WORKSPACE</div><h2 id="auth-error-title">{isSignup ? 'Không thể đăng ký' : 'Không thể đăng nhập'}</h2></div>
                            <button className="btn icon-button" onClick={() => setAuthError('')} title="Đóng" aria-label="Đóng">×</button>
                        </div>
                        <p id="auth-error-message">{authError}</p>
                        <div className="dialog-actions">
                            <button className="btn btn-primary button button-primary" onClick={() => setAuthError('')}>Đã hiểu</button>
                        </div>
                    </section>
                </div>
            )}
        </main>
    )
}
