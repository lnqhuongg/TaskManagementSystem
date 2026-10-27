import { api } from './api'

async function getErrorMessage(response, fallback) {
    try {
        const data = await response.json()
        if (data.errors) {
            const fieldNames = {
                Username: 'Tên đăng nhập',
                Email: 'Email',
                Password: 'Mật khẩu',
                ConfirmPassword: 'Xác nhận mật khẩu',
                Role: 'Vai trò',
            }
            const validationErrors = Object.entries(data.errors)
                .flatMap(([field, messages]) => {
                    const label = fieldNames[field] ?? field
                    return (Array.isArray(messages) ? messages : [messages])
                        .map((message) => `${label}: ${message}`)
                })

            if (validationErrors.length) return validationErrors.join(' ')
        }

        return data.message || data.title || fallback
    } catch {
        return fallback
    }
}

// POST /auth/login
export async function login(email, password) {
    const response = await api('/auth/login', {
        method: 'POST',
        body: JSON.stringify({
            email,
            password,
        }),
    })

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, 'Đã xảy ra lỗi khi đăng nhập.'))
    }

    return await response.json()
}

// POST /auth/register (tái sử dụng Create User logic ở Backend)
export async function register(username, email, password, confirmPassword) {
    const response = await api('/auth/register', {
        method: 'POST',
        body: JSON.stringify({
            username,
            email,
            password,
            confirmPassword,
            role: 'User',
        }),
    })

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, 'Đã xảy ra lỗi khi đăng ký.'))
    }

    return await response.json()
}