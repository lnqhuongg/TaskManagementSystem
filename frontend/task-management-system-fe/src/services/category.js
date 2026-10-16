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

    const response = await api(`/categories?${params}`)

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, 'Xảy ra lỗi khi tải danh sách danh mục công việc.'))
    }

    return await response.json()
}

// GET ALL ACTIVE /categories
export async function getAllActive() {

    const response = await api(`/categories/active`)

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, 'Xảy ra lỗi khi tải tất cả danh mục công việc đang hoạt động.'))
    }

    return await response.json()
}

// GET /categories/{id}
export async function getById(id) {
    const response = await api(`/categories/${id}`)

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, 'Xảy ra lỗi khi tải danh mục công việc.'))
    }

    return await response.json()
}

// POST /categories
export async function create(name, description = '') {

    const response = await api('/categories', {
        method: 'POST',
        body: JSON.stringify({
            name,
            description
        }),
    })

    if (!response.ok) throw new Error(await getErrorMessage(response, 'Đã xảy ra lỗi khi thêm danh mục công việc.'))

    return await response.json()
}

// PUT /categories/{id}
export async function update(name, description, status, id) {

    const response = await api(`/categories/${id}`, {
        method: 'PUT',
        body: JSON.stringify({
            name,
            description,
            status
        }),
    })

    if (!response.ok) throw new Error(await getErrorMessage(response, 'Đã xảy ra lỗi khi chỉnh sửa danh mục công việc.'))

    return await response.json()
}