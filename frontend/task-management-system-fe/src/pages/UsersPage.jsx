import { useEffect, useState } from 'react'
import { create, getAll, update } from '../services/user.js'

function normalizeUser(user) {
    const id = user.id ?? user.Id
    const email = String(user.email ?? user.Email ?? '')
    const name = String((user.name ?? user.username ?? user.Username ?? email) || 'Người dùng').trim()
    const role = String(user.role ?? user.Role ?? 'User')
    const rawStatus = user.status ?? user.Status ?? 'Active'

    return {
        ...user,
        id,
        name: name || 'Người dùng',
        email,
        role: role.toLowerCase() === 'admin' ? 'Admin' : 'User',
        status: rawStatus === 0 || String(rawStatus).toLowerCase() === 'active' ? 'Active' : 'Inactive',
    }
}

export default function UsersPage({ users, onChange, onNotice }) {
    const [search, setSearch] = useState('')
    const [page, setPage] = useState(1)
    const [editingUser, setEditingUser] = useState(null)
    const [saveError, setSaveError] = useState('')
    const [isSaving, setIsSaving] = useState(false)
    const [isLoading, setIsLoading] = useState(true)
    const [loadError, setLoadError] = useState('')
    const pageSize = 3
    const filteredUsers = users.filter((user) => `${user.name} ${user.email}`.toLowerCase().includes(search.toLowerCase()))
    const pageCount = Math.max(1, Math.ceil(filteredUsers.length / pageSize))
    const currentPage = Math.min(page, pageCount)
    const pagedUsers = filteredUsers.slice((currentPage - 1) * pageSize, currentPage * pageSize)
    const activeUsers = users.filter((user) => user.status === 'Active').length
    const adminUsers = users.filter((user) => user.role === 'Admin').length

    useEffect(() => {
        async function loadUsers() {
            setIsLoading(true)
            setLoadError('')

            try {
                const result = await getAll()
                const loadedUsers = (Array.isArray(result) ? result : result.items ?? result.data ?? []).map(normalizeUser)
                onChange(loadedUsers)
            } catch (error) {
                setLoadError(error.message || 'Không thể tải danh sách người dùng.')
            } finally {
                setIsLoading(false)
            }
        }

        loadUsers()
    }, [onChange])

    async function submitUser(event) {
        event.preventDefault()
        const form = new FormData(event.currentTarget)
        const currentUser = editingUser
        const isEditing = currentUser?.id != null
        const username = form.get('username').trim()
        const email = form.get('email').trim()
        const role = form.get('role')
        const status = isEditing ? (form.has('status') ? 'Active' : 'Inactive') : 'Active'

        if (!isEditing && form.get('password') !== form.get('confirmPassword')) {
            setSaveError('Mật khẩu và xác nhận mật khẩu không khớp.')
            return
        }

        setIsSaving(true)
        setSaveError('')
        try {
            const result = isEditing
                ? await update(username, email, role, status, currentUser.id)
                : await create(username, email, form.get('password'), form.get('confirmPassword'), role)
            const savedUser = normalizeUser(result)
            onChange(isEditing
                ? users.map((item) => item.id === savedUser.id ? savedUser : item)
                : [...users, savedUser])
            onNotice(isEditing ? 'Thông tin người dùng đã được cập nhật.' : 'Tài khoản đã được thêm.')
            setEditingUser(null)
        } catch (error) {
            setSaveError(error.message || 'Không thể lưu thông tin người dùng.')
        } finally {
            setIsSaving(false)
        }
    }

    return (
        <section className="management-view">
            <div className="page-heading">
                <div>
                    <div className="eyebrow">QUẢN TRỊ HỆ THỐNG</div>
                    <h1>Người dùng<span className="heading-period">.</span></h1>
                    <p>Quản lý tài khoản và quyền truy cập của thành viên.</p>
                </div>
                <button className="btn btn-primary button button-primary" onClick={() => setEditingUser({})}>
                    <span>＋</span> Thêm người dùng
                </button>
            </div>
            <div className="user-summary">
                <div><span>Tổng người dùng</span><strong>{users.length}</strong></div>
                <div><span>Đang hoạt động</span><strong>{activeUsers}</strong></div>
                <div><span>Quản trị viên</span><strong>{adminUsers}</strong></div>
            </div>
            <div className="table-toolbar">
                <span>Danh sách tài khoản</span>
                <label className="search-field">
                    <span>⌕</span>
                    <input value={search} onChange={(event) => {
                        setSearch(event.target.value)
                        setPage(1)
                    }} placeholder="Tìm người dùng..." aria-label="Tìm người dùng" />
                </label>
            </div>
            <div className="data-table-wrap">
                <table className="data-table">
                    <thead><tr><th>NGƯỜI DÙNG</th><th>VAI TRÒ</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead>
                    <tbody>
                        {isLoading && <tr><td colSpan="4">Đang tải người dùng...</td></tr>}
                        {!isLoading && loadError && <tr><td colSpan="4" role="alert">{loadError}</td></tr>}
                        {!isLoading && !loadError && pagedUsers.map((user) => (
                            <tr key={user.id}>
                                <td>
                                    <div className="table-user">
                                        <span className="avatar">{user.name.split(' ').map((part) => part[0]).slice(-2).join('')}</span>
                                        <span><strong>{user.name}</strong><small>{user.email}</small></span>
                                    </div>
                                </td>
                                <td><span className={`role-badge ${user.role === 'Admin' ? 'role-admin' : ''}`}>{user.role === 'Admin' ? 'Quản trị viên' : 'Người dùng'}</span></td>
                                <td><span className={`status-badge ${user.status === 'Active' ? '' : 'status-paused'}`}><i />{user.status === 'Active' ? 'Hoạt động' : 'Ngưng hoạt động'}</span></td>
                                <td>
                                    <div className="row-actions">
                                        <button className="btn icon-button" title="Chỉnh sửa người dùng" aria-label={`Chỉnh sửa ${user.name}`} onClick={() => setEditingUser(user)}>✎</button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                        {!isLoading && !loadError && !pagedUsers.length && <tr><td colSpan="4">Không có người dùng phù hợp.</td></tr>}
                    </tbody>
                </table>
            </div>
            <div className="list-footer">
                <span>
                    Hiển thị <strong>{filteredUsers.length ? (currentPage - 1) * pageSize + 1 : 0}-{Math.min(currentPage * pageSize, filteredUsers.length)}</strong>
                    {' '}trên <strong>{filteredUsers.length}</strong> người dùng
                </span>
                <div>
                    <button className="btn page-arrow" disabled={currentPage === 1} aria-label="Trang trước" onClick={() => setPage(currentPage - 1)}>‹</button>
                    <button className="btn page-number" aria-current="page">{currentPage}</button>
                    <button className="btn page-arrow" disabled={currentPage === pageCount} aria-label="Trang sau" onClick={() => setPage(currentPage + 1)}>›</button>
                </div>
            </div>
            {editingUser && <UserDialog user={editingUser} error={saveError} isSaving={isSaving} onClose={() => {
                if (!isSaving) {
                    setEditingUser(null)
                    setSaveError('')
                }
            }} onSubmit={submitUser} />}
        </section>
    )
}

function UserDialog({ user, error, isSaving, onClose, onSubmit }) {
    const isEditing = user.id != null
    const role = user.role?.toLowerCase() === 'admin' ? 'Admin' : 'User'
    const username = user.username ?? user.Username ?? user.name ?? ''

    return (
        <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose() }}>
            <section className="dialog" role="dialog" aria-modal="true" aria-labelledby="user-dialog-title">
                <div className="dialog-header">
                    <div><div className="eyebrow">TASKFLOW WORKSPACE</div><h2 id="user-dialog-title">{isEditing ? 'Chỉnh sửa người dùng' : 'Thêm người dùng mới'}</h2></div>
                    <button className="btn icon-button" onClick={onClose} title="Đóng" aria-label="Đóng">×</button>
                </div>
                <form onSubmit={onSubmit} className="dialog-form">
                    <label>Tên đăng nhập<input name="username" required minLength="3" maxLength="50" pattern="[A-Za-z0-9_]+" title="Chỉ dùng chữ cái không dấu, số và dấu gạch dưới" defaultValue={username} placeholder="Ví dụ: minh_anh" autoFocus /></label>
                    <label>Email<input type="email" name="email" required maxLength="100" defaultValue={user.email} placeholder="name@example.com" /></label>
                    {!isEditing && <>
                        <label>Mật khẩu<input type="password" name="password" required minLength="8" maxLength="100" pattern="(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#]).+" title="Cần có chữ hoa, chữ thường, số và ký tự đặc biệt (@$!%*?&#)" placeholder="Tối thiểu 8 ký tự, gồm hoa/thường, số và ký tự đặc biệt" /></label>
                        <label>Xác nhận mật khẩu<input type="password" name="confirmPassword" required minLength="8" maxLength="100" placeholder="Nhập lại mật khẩu" /></label>
                    </>}
                    <label>Vai trò<select name="role" defaultValue={role}><option value="User">Người dùng</option><option value="Admin">Quản trị viên</option></select></label>
                    {isEditing && <div className="form-check form-switch status-switch">
                        <input className="form-check-input" type="checkbox" role="switch" id="user-active" name="status" value="Active" defaultChecked={user.status !== 'Inactive'} />
                        <label className="form-check-label" htmlFor="user-active">Hoạt động</label>
                        <small>Bật để cho phép tài khoản hoạt động</small>
                    </div>}
                    {error && <div className="alert alert-danger py-2 mb-0" role="alert">{error}</div>}
                    <div className="dialog-actions">
                        <button type="button" className="btn btn-outline-secondary button button-secondary" onClick={onClose} disabled={isSaving}>Hủy</button>
                        <button className="btn btn-primary button button-primary" type="submit" disabled={isSaving}>{isSaving ? 'Đang lưu...' : isEditing ? 'Lưu thay đổi' : 'Thêm người dùng'}</button>
                    </div>
                </form>
            </section>
        </div>
    )
}
