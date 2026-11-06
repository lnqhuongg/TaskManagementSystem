import { useState, useEffect } from 'react'
import { updateProfile, changePassword } from '../services/user'

export default function ProfilePage({ account, onNotice, onUpdateUser }) {
    const [username, setUsername] = useState(account?.username ?? '')
    const [email, setEmail] = useState(account?.email ?? '')
    const [isUpdatingProfile, setIsUpdatingProfile] = useState(false)

    const [currentPassword, setCurrentPassword] = useState('')
    const [newPassword, setNewPassword] = useState('')
    const [confirmPassword, setConfirmPassword] = useState('')
    const [isChangingPassword, setIsChangingPassword] = useState(false)

    useEffect(() => {
        setUsername(account?.username ?? '')
        setEmail(account?.email ?? '')
    }, [account])

    const initials = (username || 'U')
        .split(/[\s@._-]+/)
        .filter(Boolean)
        .slice(0, 2)
        .map((part) => part[0])
        .join('')
        .toUpperCase()
    const roleText = account?.role === 'Admin' ? 'Quản trị viên' : 'Người dùng'

    async function handleProfileSubmit(e) {
        e.preventDefault()
        try {
            setIsUpdatingProfile(true)
            const updated = await updateProfile(username.trim(), email.trim())
            onNotice?.('Thông tin cá nhân đã được cập nhật thành công.')
            onUpdateUser?.(updated)
        } catch (err) {
            window.alert(err.message || 'Cập nhật thông tin thất bại.')
        } finally {
            setIsUpdatingProfile(false)
        }
    }

    async function handlePasswordSubmit(e) {
        e.preventDefault()
        if (newPassword !== confirmPassword) {
            window.alert('Xác nhận mật khẩu mới không khớp.')
            return
        }
        try {
            setIsChangingPassword(true)
            await changePassword(currentPassword, newPassword, confirmPassword)
            onNotice?.('Đổi mật khẩu thành công.')
            setCurrentPassword('')
            setNewPassword('')
            setConfirmPassword('')
        } catch (err) {
            window.alert(err.message || 'Đổi mật khẩu thất bại.')
        } finally {
            setIsChangingPassword(false)
        }
    }

    return (
        <section className="profile-view">
            <div className="page-heading">
                <div>
                    <div className="eyebrow">TÀI KHOẢN CỦA BẠN</div>
                    <h1>
                        Hồ sơ cá nhân<span className="heading-period">.</span>
                    </h1>
                    <p>Quản lý thông tin tài khoản và bảo mật mật khẩu.</p>
                </div>
            </div>

            {/* Form Cập nhật thông tin cá nhân (Chỉ có Username và Email theo đúng Database) */}
            <form className="profile-form" onSubmit={handleProfileSubmit}>
                <div className="profile-avatar-row">
                    <div className="avatar avatar-large avatar-orange">{initials}</div>
                    <div>
                        <strong>{username || 'Người dùng'}</strong>
                        <p>Vai trò: <span style={{ fontWeight: 600, color: '#333' }}>{roleText}</span></p>
                    </div>
                </div>

                <div className="form-grid">
                    <label>
                        Tên đăng nhập
                        <input
                            type="text"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            placeholder="Nhập tên đăng nhập"
                            required
                        />
                    </label>

                    <label>
                        Email
                        <input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="Nhập địa chỉ email"
                            required
                        />
                    </label>
                </div>

                <div className="form-actions">
                    <button
                        className="btn btn-primary button button-primary"
                        type="submit"
                        disabled={isUpdatingProfile}
                    >
                        {isUpdatingProfile ? 'Đang lưu...' : 'Lưu thay đổi'}
                    </button>
                </div>
            </form>

            {/* Form Đổi mật khẩu */}
            <form className="profile-form" style={{ marginTop: '24px' }} onSubmit={handlePasswordSubmit}>
                <div style={{ padding: '16px 20px', borderBottom: '1px solid var(--line)' }}>
                    <h2 style={{ fontSize: '13px', margin: 0, fontWeight: 700 }}>Đổi mật khẩu</h2>
                    <p style={{ margin: '4px 0 0', color: '#969389', fontSize: '10px' }}>
                        Mật khẩu mới tối thiểu 8 ký tự, bao gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.
                    </p>
                </div>

                <div className="form-grid">
                    <label className="full-field">
                        Mật khẩu hiện tại
                        <input
                            type="password"
                            placeholder="Nhập mật khẩu hiện tại của bạn"
                            value={currentPassword}
                            onChange={(e) => setCurrentPassword(e.target.value)}
                            required
                        />
                    </label>

                    <label>
                        Mật khẩu mới
                        <input
                            type="password"
                            placeholder="Nhập mật khẩu mới"
                            value={newPassword}
                            onChange={(e) => setNewPassword(e.target.value)}
                            required
                        />
                    </label>

                    <label>
                        Xác nhận mật khẩu mới
                        <input
                            type="password"
                            placeholder="Nhập lại mật khẩu mới"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                            required
                        />
                    </label>
                </div>

                <div className="form-actions">
                    <button
                        className="btn btn-primary button button-primary"
                        type="submit"
                        disabled={isChangingPassword}
                    >
                        {isChangingPassword ? 'Đang cập nhật...' : 'Cập nhật mật khẩu'}
                    </button>
                </div>
            </form>
        </section>
    )
}
