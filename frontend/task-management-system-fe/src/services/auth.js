import { api } from './api'

// GET /categories
export async function login(email, password) {


    const response = await api('/auth/login', {
        method: 'POST',
        body: JSON.stringify({
            email,
            password,
        }),
    })

    if (!response.ok) {
        const data = await response.json()
        throw new Error(
            data.message ||
            'Đã xảy ra lỗi khi đăng nhập.'
        )
    }

    return await response.json()
}

async function handleLogin() {
    try {
        const data = await login(email, password)

        // Lưu JWT để api.js sử dụng ở những request sau
        localStorage.setItem('taskflow_access_token', data.token)

        // Có thể lưu thêm thông tin người dùng để hiển thị giao diện
        localStorage.setItem('taskflow_user', JSON.stringify({
            username: data.username,
            email: data.email,
            role: data.role,
        }))

        console.log('Đăng nhập thành công!')
    } catch (error) {
        console.error(error.message)
    }
}