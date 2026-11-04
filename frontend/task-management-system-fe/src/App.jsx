import { useState } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import AuthPage from './pages/AuthPage.jsx'
import CategoriesPage from './pages/CategoriesPage.jsx'
import ProfilePage from './pages/ProfilePage.jsx'
import TasksPage from './pages/TasksPage.jsx'
import UsersPage from './pages/UsersPage.jsx'
import WorkspaceLayout from './WorkspaceLayout.jsx'
import './App.css'

function getUserIdFromToken(token) {
  try {
    const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    const claims = JSON.parse(atob(payload))
    const userId = claims.nameid ?? claims.sub ?? claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
    return Number(userId) || null
  } catch {
    return null
  }
}

function getStoredUser() {
  const token = localStorage.getItem('taskflow_access_token')
  const storedUser = localStorage.getItem('taskflow_user')

  if (!token || !storedUser) return null

  try {
    const user = JSON.parse(storedUser)
    if (user?.role !== 'User' && user?.role !== 'Admin') return null
    return { ...user, id: user.id ?? getUserIdFromToken(token) }
  } catch {
    return null
  }
}

function roleHome(user) {
  return user?.role === 'Admin' ? '/admin/tasks' : '/user/tasks'
}

function ProtectedWorkspace({ authUser, requiredRole, children }) {
  if (!authUser) return <Navigate to="/signin" replace />
  if (authUser.role !== requiredRole) return <Navigate to={roleHome(authUser)} replace />
  return children
}

function App() {
  const [tasks, setTasks] = useState([])
  const [users, setUsers] = useState([])
  const [categories, setCategories] = useState([])
  const [notice, setNotice] = useState('')
  const [authUser, setAuthUser] = useState(getStoredUser)

  function handleLogin(data) {
    if (!data?.token) throw new Error('Phản hồi đăng nhập không hợp lệ.')
    if (data.role !== 'User' && data.role !== 'Admin') {
      throw new Error('Không xác định được quyền của tài khoản.')
    }

    const user = {
      id: getUserIdFromToken(data.token),
      username: data.username ?? '',
      email: data.email ?? '',
      role: data.role,
    }

    localStorage.setItem('taskflow_access_token', data.token)
    localStorage.setItem('taskflow_user', JSON.stringify(user))
    setAuthUser(user)
    return user
  }

  function handleLogout() {
    localStorage.removeItem('taskflow_access_token')
    localStorage.removeItem('taskflow_user')
    setAuthUser(null)
  }

  function showNotice(message) {
    setNotice(message)
    window.setTimeout(() => setNotice(''), 2600)
  }

  return <>
    <Routes>
      <Route path="/" element={<Navigate to={authUser ? roleHome(authUser) : '/signin'} replace />} />
      <Route path="/signin" element={authUser ? <Navigate to={roleHome(authUser)} replace /> : <AuthPage mode="signin" onLogin={handleLogin} onNotice={showNotice} />} />
      <Route path="/register" element={authUser ? <Navigate to={roleHome(authUser)} replace /> : <AuthPage mode="register" onLogin={handleLogin} onNotice={showNotice} />} />
      <Route path="/user" element={<ProtectedWorkspace authUser={authUser} requiredRole="User"><WorkspaceLayout role="user" account={authUser} onLogout={handleLogout} /></ProtectedWorkspace>}>
        <Route index element={<Navigate to="tasks" replace />} />
        <Route path="tasks" element={<TasksPage role="user" userId={authUser?.id} username={authUser?.username} tasks={tasks} onChange={setTasks} onNotice={showNotice} />} />
        <Route path="profile" element={<ProfilePage account={authUser} onNotice={showNotice} />} />
      </Route>
      <Route path="/admin" element={<ProtectedWorkspace authUser={authUser} requiredRole="Admin"><WorkspaceLayout role="admin" account={authUser} onLogout={handleLogout} /></ProtectedWorkspace>}>
        <Route index element={<Navigate to="tasks" replace />} />
        <Route path="tasks" element={<TasksPage role="admin" userId={authUser?.id} username={authUser?.username} tasks={tasks} onChange={setTasks} onNotice={showNotice} />} />
        <Route path="categories" element={<CategoriesPage categories={categories} onChange={setCategories} onNotice={showNotice} />} />
        <Route path="users" element={<UsersPage users={users} onChange={setUsers} onNotice={showNotice} />} />
        <Route path="profile" element={<ProfilePage account={authUser} onNotice={showNotice} />} />
      </Route>
      <Route path="*" element={<Navigate to="/signin" replace />} />
    </Routes>
    {notice && <div className="toast" role="status"><span>✓</span>{notice}</div>}
  </>
}

export default App
