import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'

const userNavigation = [
    { path: '/user/tasks', label: 'Công việc', icon: '▤' },
    { path: '/user/profile', label: 'Hồ sơ cá nhân', icon: '◎' },
]

const adminNavigation = [
    { path: '/admin/tasks', label: 'Công việc', icon: '▤' },
    { path: '/admin/categories', label: 'Danh mục', icon: '◫' },
    { path: '/admin/users', label: 'Người dùng', icon: '♙' },
]

export default function WorkspaceLayout({ role, account, onLogout }) {
    const location = useLocation()
    const navigate = useNavigate()
    const navigation = role === 'admin' ? adminNavigation : userNavigation
    const username = account?.username || account?.email || 'Tài khoản'
    const initials = username.split(/[\s@._-]+/).filter(Boolean).slice(0, 2).map((part) => part[0]).join('').toUpperCase()
    const currentPage = location.pathname.endsWith('/profile')
        ? 'Hồ sơ cá nhân'
        : navigation.find((item) => item.path === location.pathname)?.label ?? 'Công việc'

    return <div className="app-shell">
        <aside className="sidebar">
            <NavLink className="brand" to={role === 'admin' ? '/admin/tasks' : '/user/tasks'}><span className="brand-mark">t.</span><span>taskflow<small>WORKSPACE</small></span></NavLink>
            {/* <div className="workspace-label">KHÔNG GIAN LÀM VIỆC</div>
            <div className="workspace-switch"><span className="workspace-avatar">T</span><span>Không gian cá nhân<small>Gói cá nhân</small></span></div> */}
            <div className="nav-label">MENU</div>
            <nav className="main-nav" aria-label="Điều hướng chính">
                {navigation.map((item) => <NavLink key={item.path} to={item.path} className={({ isActive }) => `nav-item ${isActive ? 'selected' : ''}`}><span className="nav-icon" aria-hidden="true">{item.icon}</span>{item.label}</NavLink>)}
                {role === 'admin' && <NavLink to="/admin/profile" className={({ isActive }) => `nav-item ${isActive ? 'selected' : ''}`}><span className="nav-icon" aria-hidden="true">◎</span>Hồ sơ cá nhân</NavLink>}
            </nav>
            <div className="sidebar-bottom">
                <div className="sidebar-user">
                    <div className="avatar avatar-orange">{initials}</div>
                    <div className="user-meta"><strong>{username}</strong><span>{role === 'admin' ? 'Admin' : 'User'}</span></div>
                    <button className="btn icon-button logout-button" title="Đăng xuất" aria-label="Đăng xuất" onClick={() => {
                        onLogout()
                        navigate('/signin')
                    }}>↪</button>
                </div>
            </div>
        </aside>
        <main className="main-area">
            <header className="topbar">
                <div className="breadcrumbs">
                    <span>Workspace</span>
                    <span className="crumb-separator">/</span>
                    <strong>{currentPage}</strong>
                </div>
            </header>
            <div className="content-scroll"><div className="page-content"><Outlet /></div><footer className="app-footer"><span>© 2026 Taskflow</span><span>Đơn giản hóa từng việc cần làm.</span></footer></div>
        </main>
    </div>
}
