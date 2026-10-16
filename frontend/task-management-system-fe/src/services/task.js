import { api } from './api'

async function getErrorMessage(response, fallback) {
    try {
        const data = await response.json()
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

    const response = await api(`/tasks?${params}`)

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, 'Xảy ra lỗi khi tải danh sách công việc.'))
    }

    return await response.json()
}

// POST /tasks
export async function create(title, description, status, priority, dueDate, userId, categoryId) {
    const response = await api('/tasks', {
        method: 'POST',
        body: JSON.stringify({
            title,
            description,
            status,
            priority,
            dueDate,
            userId,
            categoryId,
        }),
    })

    if (!response.ok) throw new Error(await getErrorMessage(response, 'Đã xảy ra lỗi khi thêm công việc.'))

    return await response.json()
}

// PUT /tasks/{id}
export async function update(title, description, status, priority, dueDate, userId, categoryId, id) {
    const response = await api(`/tasks/${id}`, {
        method: 'PUT',
        body: JSON.stringify({
            title,
            description,
            status,
            priority,
            dueDate,
            userId,
            categoryId,
        }),
    })

    if (!response.ok) throw new Error(await getErrorMessage(response, 'Đã xảy ra lỗi khi chỉnh sửa công việc.'))

    return await response.json()
}

// GET /categories/{id}
export async function getById(id) {
    const response = await api(`/tasks/${id}`)

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, 'Xảy ra lỗi khi tải công việc.'))
    }

    return await response.json()
}