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

// GET /categories
export async function getAll(page = 1, pageSize = 5, keyword = '') {
    const params = new URLSearchParams({
        page,
        pageSize,
        keyword,
    })

    const response = await api(`/users?${params}`)

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, 'Xảy ra lỗi khi tải danh sách người dùng.'))
    }

    return await response.json()
}

// POST /users
export async function create(username, email, password, confirmPassword, role = 'User') {
    const response = await api('/users', {
        method: 'POST',
        body: JSON.stringify({
            username,
            email,
            password,
            confirmPassword,
            role,
        }),
    })

    if (!response.ok) throw new Error(await getErrorMessage(response, 'Đã xảy ra lỗi khi thêm người dùng.'))

    return await response.json()
}

// PUT /users/{id}
export async function update(username, email, role, status, id) {
    const response = await api(`/users/${id}`, {
        method: 'PUT',
        body: JSON.stringify({
            username,
            email,
            role,
            status,
        }),
    })

    if (!response.ok) throw new Error(await getErrorMessage(response, 'Đã xảy ra lỗi khi chỉnh sửa người dùng.'))

    return await response.json()
}

// GET /categories/{id}
export async function getById(id) {
    const response = await api(`/users/${id}`)

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, 'Xảy ra lỗi khi tải thông tin người dùng.'))
    }

    return await response.json()
}