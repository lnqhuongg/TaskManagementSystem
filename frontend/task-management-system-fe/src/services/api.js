/*
 * MỘT FUNCTION DÙNG CHUNG ĐỂ REACT GỌI API ĐẾN BACKEND 
 */

// Địa chỉ API
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7284'

/*
 * Function để các file services khác có thể import và sử dụng 
 * path -> Đường dẫn của API muốn gọi, ví dụ /categories
 * options = {} -> Các tùy chọn cho request, ví dụ HTTP method, request body và headers. Nếu không truyền thì mặc định là object rỗng {}.
*/

export function api(path, options = {}) {
  // Lấy token từ localStorage
  const token = localStorage.getItem('taskflow_access_token')

  // Gửi HTTP request = fetch()
  return fetch(`${API_BASE_URL}${path}`, /* Ghép URL */ {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
      ...options.headers,
    },
  })
}