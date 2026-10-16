import { useEffect, useMemo, useState } from 'react'
import { create, getAll, update } from '../services/task.js'
import { getAllActive } from '../services/category.js'

const today = new Date().toISOString().slice(0, 10)

const taskStatuses = [
    { value: 'Todo', label: 'Chưa bắt đầu' },
    { value: 'InProgress', label: 'Đang thực hiện' },
    { value: 'Completed', label: 'Hoàn thành' },
]

const taskPriorities = [
    { value: 'Low', label: 'Thấp' },
    { value: 'Medium', label: 'Trung bình' },
    { value: 'High', label: 'Cao' },
]

const todayLabel = new Intl.DateTimeFormat('vi-VN', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
}).format(new Date()).toLocaleUpperCase('vi-VN')

function normalizeCategory(category) {
    const rawStatus = category.status ?? category.Status ?? 'Active'
    return {
        id: Number(category.id ?? category.Id),
        name: String(category.name ?? category.Name ?? ''),
        status: rawStatus === 0 || String(rawStatus).toLowerCase() === 'active' ? 'Active' : 'Inactive',
    }
}

function normalizeTask(task) {
    const rawStatus = task.status ?? task.Status
    const status = typeof rawStatus === 'number'
        ? taskStatuses[rawStatus]?.value ?? 'Todo'
        : taskStatuses.find((item) => item.value.toLowerCase() === String(rawStatus ?? '').toLowerCase())?.value
            ?? (task.completed ?? task.Completed ? 'Completed' : 'Todo')
    const rawPriority = task.priority ?? task.Priority
    const priority = typeof rawPriority === 'number'
        ? taskPriorities[rawPriority]?.value ?? 'Medium'
        : taskPriorities.find((item) => item.value.toLowerCase() === String(rawPriority ?? '').toLowerCase())?.value ?? 'Medium'

    return {
        ...task,
        id: task.id ?? task.Id,
        title: String(task.title ?? task.Title ?? ''),
        category: String(task.category ?? task.categoryName ?? task.CategoryName ?? ''),
        categoryId: task.categoryId ?? task.CategoryId,
        dueDate: String(task.dueDate ?? task.DueDate ?? '').slice(0, 10),
        createdAt: task.createdAt ?? task.CreatedAt,
        owner: task.owner ?? task.userName ?? task.UserName ?? '',
        userId: task.userId ?? task.UserId,
        status,
        priority,
    }
}

function statusLabel(status) {
    return taskStatuses.find((item) => item.value === status)?.label ?? taskStatuses[0].label
}

function priorityLabel(priority) {
    return taskPriorities.find((item) => item.value === priority)?.label ?? taskPriorities[1].label
}

export default function TasksPage({ role, userId, username, tasks, onChange, onNotice }) {
    const [filter, setFilter] = useState('Tất cả')
    const [categoryFilter, setCategoryFilter] = useState('Tất cả danh mục')
    const [search, setSearch] = useState('')
    const [editingTask, setEditingTask] = useState(null)
    const [page, setPage] = useState(1)
    const [isLoading, setIsLoading] = useState(true)
    const [loadError, setLoadError] = useState('')
    const [activeCategories, setActiveCategories] = useState([])
    const [categoryLoadError, setCategoryLoadError] = useState('')
    const [saveError, setSaveError] = useState('')
    const [isSaving, setIsSaving] = useState(false)
    const pageSize = 3
    const activeCount = tasks.filter((task) => task.status !== 'Completed').length
    const completedCount = tasks.filter((task) => task.status === 'Completed').length
    const visibleTasks = useMemo(() => tasks.filter((task) => {
        const matchesStatus = filter === 'Tất cả' || (filter === 'Đang làm' && task.status !== 'Completed') || (filter === 'Hoàn thành' && task.status === 'Completed')
        const matchesCategory = categoryFilter === 'Tất cả danh mục' || task.category === categoryFilter
        const matchesSearch = task.title.toLowerCase().includes(search.toLowerCase()) || task.category.toLowerCase().includes(search.toLowerCase())
        return matchesStatus && matchesCategory && matchesSearch
    }), [tasks, filter, categoryFilter, search])
    const pageCount = Math.max(1, Math.ceil(visibleTasks.length / pageSize))
    const currentPage = Math.min(page, pageCount)
    const pagedTasks = visibleTasks.slice((currentPage - 1) * pageSize, currentPage * pageSize)
    const dialogCategories = editingTask?.id != null
        && editingTask.categoryId != null
        && !activeCategories.some((category) => category.id === Number(editingTask.categoryId))
        ? [{ id: Number(editingTask.categoryId), name: editingTask.category || 'Danh mục hiện tại' }, ...activeCategories]
        : activeCategories

    useEffect(() => {
        async function loadTasks() {
            setIsLoading(true)
            setLoadError('')

            try {
                const result = await getAll()
                const loadedTasks = (Array.isArray(result) ? result : result.items ?? result.data ?? []).map(normalizeTask)
                onChange(loadedTasks)
            } catch (error) {
                setLoadError(error.message || 'Không thể tải công việc.')
            } finally {
                setIsLoading(false)
            }
        }

        loadTasks()
    }, [onChange])

    useEffect(() => {
        async function loadActiveCategories() {
            setCategoryLoadError('')

            try {
                const result = await getAllActive()
                const loadedCategories = (Array.isArray(result) ? result : result.items ?? result.data ?? result.Data ?? [])
                    .map(normalizeCategory)
                    .filter((category) => category.status === 'Active')
                setActiveCategories(loadedCategories)
            } catch (error) {
                setCategoryLoadError(error.message || 'Không thể tải danh mục đang hoạt động.')
            }
        }

        loadActiveCategories()
    }, [])

    async function submitTask(event) {
        event.preventDefault()
        const form = new FormData(event.currentTarget)
        const taskUserId = editingTask?.userId ?? userId
        const categoryId = Number(form.get('categoryId'))
        const category = activeCategories.find((item) => item.id === categoryId)
        const isCurrentCategory = editingTask?.id != null && categoryId === Number(editingTask.categoryId)

        if (!taskUserId) {
            setSaveError('Không xác định được tài khoản hiện tại. Vui lòng đăng nhập lại.')
            return
        }

        if (!categoryId || (!category && !isCurrentCategory)) {
            setSaveError(categoryLoadError || (activeCategories.length
                ? 'Vui lòng chọn một danh mục đang hoạt động.'
                : 'Chưa có danh mục nào đang hoạt động để tạo công việc.'))
            return
        }

        setIsSaving(true)
        setSaveError('')
        const values = {
            title: form.get('title').trim(),
            description: form.get('description').trim(),
            status: editingTask?.id != null ? form.get('status') : 'Todo',
            priority: form.get('priority'),
            dueDate: form.get('dueDate') || null,
            userId: taskUserId,
            categoryId,
        }

        try {
            const result = editingTask?.id != null
                ? await update(values.title, values.description, values.status, values.priority, values.dueDate, values.userId, values.categoryId, editingTask.id)
                : await create(values.title, values.description, values.status, values.priority, values.dueDate, values.userId, values.categoryId)
            const savedTask = normalizeTask(result)
            savedTask.category = category?.name ?? editingTask.category
            onChange(editingTask?.id != null
                ? tasks.map((item) => item.id === savedTask.id ? savedTask : item)
                : [savedTask, ...tasks])
            onNotice(editingTask?.id != null ? 'Đã cập nhật công việc.' : 'Đã thêm công việc mới.')
            setEditingTask(null)
        } catch (error) {
            setSaveError(error.message || 'Không thể lưu công việc.')
        } finally {
            setIsSaving(false)
        }
    }

    return (
        <>
            <section className="page-heading">
                <div>
                    <div className="eyebrow">{todayLabel}</div>
                    <h1>{role === 'admin' ? 'Tổng quan công việc' : `Chào mừng${username ? `, ${username}` : ''}`}<span className="heading-period">.</span></h1>
                    <p>{role === 'admin' ? 'Theo dõi và quản lý công việc của toàn hệ thống.' : 'Mỗi việc nhỏ hoàn thành là một bước tiến lớn.'}</p>
                </div>
            </section>
            <section className="stat-grid" aria-label="Thống kê công việc">
                <StatCard label="Tổng công việc" value={tasks.length} helper="Trong danh sách của bạn" icon="▤" tone="orange" />
                <StatCard label="Đang thực hiện" value={activeCount} helper="Cần bạn hoàn thành" icon="◷" tone="green" />
                <StatCard label="Đã hoàn thành" value={completedCount} helper="Bạn đang làm rất tốt" icon="✓" tone="blue" />
            </section>
            <section className="task-section">
                <div className="section-heading">
                    <div><h2>{role === 'admin' ? 'Tất cả công việc' : 'Danh sách công việc'}</h2><p>{visibleTasks.length} công việc được hiển thị</p></div>
                    {role === 'user' && <button className="btn btn-primary button button-primary" onClick={() => setEditingTask({})}>＋ Thêm công việc</button>}
                </div>
                <div className="filter-row">
                    <div className="filter-tabs" role="tablist" aria-label="Lọc trạng thái">
                        {['Tất cả', 'Đang làm', 'Hoàn thành'].map((item) => (
                            <button
                                key={item}
                                className={`btn ${filter === item ? 'active' : ''}`}
                                role="tab"
                                aria-selected={filter === item}
                                onClick={() => {
                                    setFilter(item)
                                    setPage(1)
                                }}
                            >
                                {item}{item === 'Đang làm' && <span>{activeCount}</span>}
                            </button>
                        ))}
                    </div>
                    <div className="filter-controls">
                        <label className="search-field">
                            <span aria-hidden="true">⌕</span>
                            <input value={search} onChange={(event) => {
                                setSearch(event.target.value)
                                setPage(1)
                            }} placeholder="Tìm công việc..." aria-label="Tìm công việc" />
                        </label>
                        <select className="select-control" value={categoryFilter} onChange={(event) => {
                            setCategoryFilter(event.target.value)
                            setPage(1)
                        }} aria-label="Lọc theo danh mục">
                            <option>Tất cả danh mục</option>
                            {activeCategories.map((category) => <option key={category.id}>{category.name}</option>)}
                        </select>
                        <div className="btn icon-button filter-button" title="Bộ lọc" aria-label="Bộ lọc">☷</div>
                    </div>
                </div>
                {role === 'admin' ? (
                    <div className="data-table-wrap task-admin-table">
                        <table className="data-table">
                            <thead>
                                <tr>
                                    <th>CÔNG VIỆC</th>
                                    <th>NGƯỜI TẠO</th>
                                    <th>DANH MỤC</th>
                                    <th>HẠN HOÀN THÀNH</th>
                                    <th>NGÀY TẠO</th>
                                    <th>ƯU TIÊN</th>
                                    <th>TRẠNG THÁI</th>
                                </tr>
                            </thead>
                            <tbody>
                                {isLoading && <tr><td colSpan="7">Đang tải công việc...</td></tr>}
                                {!isLoading && loadError && <tr><td colSpan="7" role="alert">{loadError}</td></tr>}
                                {!isLoading && !loadError && pagedTasks.map((task) => (
                                    <tr key={task.id}>
                                        <td>{task.title}</td>
                                        <td>{task.owner || 'Không rõ'}</td>
                                        <td>{task.category}</td>
                                        <td>{formatDate(task.dueDate)}</td>
                                        <td>{formatDate(task.createdAt)}</td>
                                        <td><span className={`priority priority-${task.priority.toLowerCase()}`}><i />{priorityLabel(task.priority)}</span></td>
                                        <td><TaskStatusBadge status={task.status} /></td>
                                    </tr>
                                ))}
                                {!isLoading && !loadError && !pagedTasks.length && <tr><td colSpan="7">Hiện chưa có công việc nào.</td></tr>}
                            </tbody>
                        </table>
                    </div>
                ) : (
                    <div className="task-list">
                        {isLoading
                            ? <div className="empty-state"><strong>Đang tải công việc...</strong></div>
                            : loadError
                                ? <div className="empty-state" role="alert"><strong>{loadError}</strong></div>
                                : pagedTasks.length
                                    ? pagedTasks.map((task) => (
                                        <TaskRow key={task.id} task={task} onEdit={() => setEditingTask(task)} />
                                    ))
                                    : <div className="empty-state"><span>⌕</span><strong>Chưa tìm thấy công việc</strong><p>Thử đổi từ khóa hoặc bộ lọc nhé.</p></div>}
                    </div>
                )}
                <div className="list-footer">
                    <span>
                        Hiển thị <strong>{visibleTasks.length ? (currentPage - 1) * pageSize + 1 : 0}-{Math.min(currentPage * pageSize, visibleTasks.length)}</strong>
                        {' '}trên <strong>{visibleTasks.length}</strong> công việc
                    </span>
                    <div>
                        <button className="btn page-arrow" disabled={currentPage === 1} aria-label="Trang trước" onClick={() => setPage(currentPage - 1)}>‹</button>
                        <button className="btn page-number" aria-current="page">{currentPage}</button>
                        <button className="btn page-arrow" disabled={currentPage === pageCount} aria-label="Trang sau" onClick={() => setPage(currentPage + 1)}>›</button>
                    </div>
                </div>
            </section>
            {editingTask && <TaskDialog task={editingTask} categories={dialogCategories} categoryLoadError={categoryLoadError} error={saveError} isSaving={isSaving} onClose={() => {
                if (!isSaving) {
                    setEditingTask(null)
                    setSaveError('')
                }
            }} onSubmit={submitTask} />}
        </>
    )
}

function StatCard({ label, value, helper, icon, tone }) {
    return (
        <article className="stat-card">
            <div className={`stat-icon ${tone}`}>{icon}</div>
            <div className="stat-copy"><span>{label}</span><strong>{value}</strong><small>{helper}</small></div>
            <span className="stat-trend">↗</span>
        </article>
    )
}

function TaskRow({ task, onEdit }) {
    const overdue = task.status !== 'Completed' && task.dueDate && task.dueDate < today
    const createdDate = formatDate(task.createdAt)

    return (
        <article className={`task-row ${task.status === 'Completed' ? 'task-completed' : ''}`}>
            <div className="task-main">
                <strong>{task.title}</strong>
                <div className="task-meta">
                    <span className="category-tag"><i />{task.category}</span>
                    <TaskStatusBadge status={task.status} />
                    <span className={overdue ? 'due-date overdue' : 'due-date'}>
                        <span aria-hidden="true">◷</span>{task.dueDate ? `${overdue ? 'Quá hạn' : 'Hạn'} ${new Date(`${task.dueDate}T00:00:00`).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' })}` : 'Chưa có hạn'}
                    </span>
                    <span className="task-created">Ngày tạo: {createdDate}</span>
                </div>
            </div>
            <span className={`priority priority-${task.priority.toLowerCase()}`}><i />{priorityLabel(task.priority)}</span>
            <div className="row-actions">
                <button className="btn icon-button" title="Chỉnh sửa công việc" aria-label="Chỉnh sửa công việc" onClick={onEdit}>✎</button>
            </div>
        </article>
    )
}

function TaskStatusBadge({ status }) {
    return <span className={`status-badge task-status-${status.toLowerCase()}`}><i />{statusLabel(status)}</span>
}

function formatDate(value) {
    return value ? new Date(value).toLocaleDateString('vi-VN') : 'Chưa có'
}

function TaskDialog({ task, categories, categoryLoadError, error, isSaving, onClose, onSubmit }) {
    const isEditing = task.id != null

    return (
        <div className="dialog-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose() }}>
            <section className="dialog" role="dialog" aria-modal="true" aria-labelledby="dialog-title">
                <div className="dialog-header">
                    <div><div className="eyebrow">TASKFLOW WORKSPACE</div><h2 id="dialog-title">{isEditing ? 'Chỉnh sửa công việc' : 'Tạo công việc mới'}</h2></div>
                    <button className="btn icon-button" onClick={onClose} title="Đóng" aria-label="Đóng">×</button>
                </div>
                <form onSubmit={onSubmit} className="dialog-form">
                    <label>Tên công việc<input name="title" required defaultValue={task.title} placeholder="Ví dụ: Hoàn thành báo cáo" autoFocus /></label>
                    <div className="form-grid">
                        <label>Danh mục<select name="categoryId" required defaultValue={task.categoryId ?? categories.find((category) => category.name === task.category)?.id ?? ''}><option value="" disabled>{categoryLoadError ? 'Không tải được danh mục' : categories.length ? 'Chọn danh mục' : 'Chưa có danh mục hoạt động'}</option>{categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label>
                        <label>Ngày đến hạn<input type="date" name="dueDate" defaultValue={task.dueDate ?? ''} /></label>
                    </div>
                    <label>Mô tả<textarea name="description" maxLength="1000" rows="3" defaultValue={task.description ?? task.Description ?? ''} placeholder="Mô tả công việc (tối đa 1000 ký tự)" /></label>
                    {isEditing && <label>Trạng thái<select name="status" defaultValue={task.status ?? 'Todo'}>{taskStatuses.map((status) => <option key={status.value} value={status.value}>{status.label}</option>)}</select></label>}
                    <label>Mức độ ưu tiên<select name="priority" defaultValue={task.priority ?? 'Medium'}>{taskPriorities.map((priority) => <option key={priority.value} value={priority.value}>{priority.label}</option>)}</select></label>
                    {error && <div className="alert alert-danger py-2 mb-0" role="alert">{error}</div>}
                    <div className="dialog-actions">
                        <button type="button" className="btn btn-outline-secondary button button-secondary" onClick={onClose} disabled={isSaving}>Hủy</button>
                        <button className="btn btn-primary button button-primary" type="submit" disabled={isSaving}>{isSaving ? 'Đang lưu...' : isEditing ? 'Lưu thay đổi' : 'Tạo công việc'}</button>
                    </div>
                </form>
            </section>
        </div>
    )
}
