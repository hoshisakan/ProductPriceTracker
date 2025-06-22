import axios from 'axios';

const api = axios.create({
    baseURL: 'http://localhost/api', // 依實際 API 地址修改
    // baseURL: 'http://localhost:5000/api', // 依實際 API 地址修改
    headers: {
        'Content-Type': 'application/json',
    },
});

export const setAuthToken = (accessToken, refreshToken) => {
    if (accessToken) {
        api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
        localStorage.setItem('accessToken', accessToken);
        if (refreshToken) {
            localStorage.setItem('refreshToken', refreshToken);
        }
    } else {
        delete api.defaults.headers.common['Authorization'];
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
    }
};

api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('accessToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        } else {
            delete config.headers.Authorization; // 如果沒有 token，則刪除 Authorization 標頭
        }
        return config;
    },
    (error) => Promise.reject(error)
);

api.interceptors.response.use(
    (res) => res,
    async (error) => {
        const originalRequest = error.config;
        const refreshToken = localStorage.getItem('refreshToken');
        if (error.response?.status === 401 && !originalRequest._retry && refreshToken) {
            originalRequest._retry = true;
            try {
                const res = await axios.post('http://localhost/api/auth/refresh', { refreshToken });
                const newAccessToken = res.data.accessToken;
                localStorage.setItem('accessToken', newAccessToken);
                originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
                return api(originalRequest);
            } catch (refreshError) {
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                window.location.href = '/login';
            }
        }
        return Promise.reject(error);
    }
);

export default api;
