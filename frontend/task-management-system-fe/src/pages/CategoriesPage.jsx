import { useEffect, useState } from 'react'
import { create, getAll, update } from '../services/category.js'

function normalizeCategory(category) {
    const status = category.status ?? category.Status ?? 'Active'
    return {
        ...category,
        id: category.id ?? category.Id,
        name: String(category.name ?? category.Name ?? ''),
        status: status === 0 || String(status).toLowerCase() === 'active' ? 'Active' : 'Inactive',
    }
}

export default function CategoriesPage({ categories, onChange, onNotice }) {
    const [search, setSearch] = useState('')
    const [page, setPage] = useState(1)
    const [editingCategory, setEditingCategory] = useState(null)
    const [saveError, setSaveError] = useState('')
    const [isSaving, setIsSaving] = useState(false)
    const [isLoading, setIsLoading] = useState(true)
    const [loadError, setLoadError] = useState('')
    const pageSize = 3
    const filteredCategories = categories.filter((category) => category.name.toLowerCase().includes(search.toLowerCase()))
    const pageCount = Math.max(1, Math.ceil(filteredCategories.length / pageSize))
    const currentPage = Math.min(page, pageCount)
    const pagedCategories = filteredCategories.slice((currentPage - 1) * pageSize, currentPage * pageSize)

    useEffect(() => {
        async function loadCategories() {
            setIsLoading(true)
            setLoadError('')

            try {
                const result = await getAll()
                const loadedCategories = (Array.isArray(result) ? result : result.items ?? result.data ?? []).map(normalizeCategory)
                onChange(loadedCategories)
            } catch (error) {
                setLoadError(error.message || 'Không thể tải danh mục.')
            } finally {
                setIsLoading(false)
            }
        }

        loadCategories()
    }, [onChange])

    async function submitCategory(event) {
        event.preventDefault()
        const form = new FormData(event.currentTarget)
        const currentCategory = editingCategory
        const name = form.get('name').trim()
        const description = form.get('description').trim()
        const status = currentCategory?.id != null ? (form.has('status') ? 'Active' : 'Inactive') : 'Active'

        setIsSaving(true)
        setSaveError('')
        try {
            if (currentCategory?.id != null) {
                const updatedCategory = normalizeCategory(await update(name, description, status, currentCategory.id))
                onChange(categories.map((item) => item.id === currentCategory.id ? { ...item, ...updatedCategory } : item))
                onNotice('Danh mục đã được cập nhật.')
            } else {
                const createdCategory = normalizeCategory(await create(name, description))
                onChange([...categories, { color: '#d97537', count: 0, ...createdCategory }])
                onNotice('Danh mục đã được tạo.')
            }
            setEditingCategory(null)
        } catch (error) {
            setSaveError(error.message || 'Không thể lưu danh mục.')
        } finally {
            setIsSaving(false)
        }
    }

    return (
        <section className="management-view">
            <div className="page-heading">
                <div>
                    <div className="eyebrow">QUẢN TRỊ HỆ THỐNG</div>
                    <h1>Danh mục<span className="heading-period">.</span></h1>
                    <p>Quản lý các phân loại công việc trong hệ thống.</p>
                </div>
                <button className="btn btn-primary button button-primary" onClick={() => setEditingCategory({})}>
                    <span>＋</span> Thêm danh mục
                </button>
            </div>
            <div className="table-toolbar">
                <span>{filteredCategories.length} danh mục</span>
                <label className="search-field">
                    <span>⌕</span>
                    <input value={search} onChange={(event) => {
                        setSearch(event.target.value)
                        setPage(1)
                    }} placeholder="Tìm danh mục..." aria-label="Tìm danh mục" />
                </label>
            </div>
            <div className="data-table-wrap">
                <table className="data-table">
                    <thead><tr><th>MÃ DANH MỤC</th><th>TÊN DANH MỤC</th><th>TRẠNG THÁI</th><th className="text-center">THAO TÁC</th></tr></thead>
                    <tbody>
                        {isLoading && <tr><td colSpan="4">Đang tải danh mục...</td></tr>}
                        {!isLoading && loadError && <tr><td colSpan="4" role="alert">{loadError}</td></tr>}
                        {!isLoading && !loadError && pagedCategories.map((category) => (
                            <tr key={category.id}>
                                <td><span className="category-id">{category.id}</span></td>
                                <td><span className="category-name">{category.name}</span></td>
                                <td><span className={`status-badge ${category.status === 'Active' ? '' : 'status-paused'}`}><i />{category.status === 'Active' ? 'Hoạt động' : 'Ngưng hoạt động'}</span></td>
                                <td className="text-center">
                                    <div className="row-actions justify-content-center">
                                        <button className="btn icon-button" title="Chỉnh sửa" aria-label={`Chỉnh sửa ${category.name}`} onClick={() => setEditingCategory(category)}>✎</button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                        {!isLoading && !loadError && !pagedCategories.length && <tr><td colSpan="4">Không có danh mục phù hợp.</td></tr>}
                    </tbody>
                </table>
            </div>
            <div className="list-footer">
                <span>
                    Hiển thị <strong>{filteredCategories.length ? (currentPage - 1) * pageSize + 1 : 0}-{Math.min(currentPage * pageSize, filteredCategories.length)}</strong>
                    {' '}trên <strong>{filteredCategories.length}</strong> danh mục
                </span>
                <div>
                    <button className="btn page-arrow" disabled={currentPage === 1} aria-label="Trang trước" onClick={() => setPage(currentPage - 1)}>‹</button>
                    <button className="btn page-number" aria-current="page">{currentPage}</button>
                    <button className="btn page-arrow" disabled={currentPage === pageCount} aria-label="Trang sau" onClick={() => setPage(currentPage + 1)}>›</button>
                </div>
            </div>
            {editingCategory && <CategoryDialog category={editingCategory} error={saveError} isSaving={isSaving} onClose={() => {
                setEditingCategory(null)
                setSaveError('')
            }} onSubmit={submitCategory} />}
        </section>
    )
}

function CategoryDialog({ category, error, isSaving, onClose, onSubmit }) {
    const isEditing = category.id != null

    return (
        <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose() }}>
            <section className="dialog" role="dialog" aria-modal="true" aria-labelledby="category-dialog-title">
                <div className="dialog-header">
                    <div><div className="eyebrow">TASKFLOW WORKSPACE</div><h2 id="category-dialog-title">{isEditing ? 'Chỉnh sửa danh mục' : 'Thêm danh mục mới'}</h2></div>
                    <button className="btn icon-button" onClick={onClose} title="Đóng" aria-label="Đóng">×</button>
                </div>
                <form onSubmit={onSubmit} className="dialog-form">
                    <label>Tên danh mục<input name="name" required minLength="2" maxLength="100" defaultValue={category.name} placeholder="Nhập tên danh mục" autoFocus /></label>
                    <label>Mô tả<textarea name="description" maxLength="500" rows="3" defaultValue={category.description ?? category.Description ?? ''} placeholder="Mô tả ngắn về danh mục (tối đa 500 ký tự)" /></label>
                    {isEditing && <div className="form-check form-switch status-switch">
                        <input className="form-check-input" type="checkbox" role="switch" id="category-active" name="status" value="Active" defaultChecked={category.status !== 'Inactive'} />
                        <label className="form-check-label" htmlFor="category-active">Hoạt động</label>
                        <small>Bật để danh mục được sử dụng</small>
                    </div>}
                    {error && <div className="alert alert-danger py-2 mb-0" role="alert">{error}</div>}
                    <div className="dialog-actions">
                        <button type="button" className="btn btn-outline-secondary button button-secondary" onClick={onClose} disabled={isSaving}>Hủy</button>
                        <button className="btn btn-primary button button-primary" type="submit" disabled={isSaving}>{isSaving ? 'Đang lưu...' : isEditing ? 'Lưu thay đổi' : 'Thêm danh mục'}</button>
                    </div>
                </form>
            </section>
        </div>
    )
}
