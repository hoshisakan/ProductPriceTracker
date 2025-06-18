import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getToken, logout, isAuthenticated } from '../services/auth';
import axios from 'axios';

export default function CrawlerRequest() {
    const [mode, setMode] = useState('pchome'); // 預設選項
    const [keyword, setKeyword] = useState('');
    const [maxPage, setMaxPage] = useState(1);
    const [message, setMessage] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        if (!isAuthenticated()) {
            navigate('/login');
        }
    }, [navigate]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        const token = getToken();
        try {
            await axios.post(
                'http://localhost:5003/api/crawlrequests',
                { mode, keyword, maxPage: Number(maxPage) },
                { headers: { Authorization: `Bearer ${token}` } }
            );
            setMessage('任務已送出！');
        } catch (error) {
            setMessage('送出失敗，請稍後再試');
        }
    };

    return (
        <div className="container mt-4">
            <button
                className="btn btn-danger mb-3"
                onClick={() => {
                    logout();
                    navigate('/login');
                }}
            >
                登出
            </button>

            <h1 className="mb-4">建立爬蟲任務</h1>

            <form onSubmit={handleSubmit}>
                <div className="mb-3">
                    <label htmlFor="modeSelect" className="form-label">
                        Mode：
                    </label>
                    <select
                        id="modeSelect"
                        className="form-select"
                        value={mode}
                        onChange={(e) => setMode(e.target.value)}
                        required
                    >
                        <option value="pchome">pchome</option>
                        <option value="momo">momo</option>
                    </select>
                </div>

                <div className="mb-3">
                    <label htmlFor="keywordInput" className="form-label">
                        Keyword：
                    </label>
                    <input
                        type="text"
                        id="keywordInput"
                        className="form-control"
                        value={keyword}
                        onChange={(e) => setKeyword(e.target.value)}
                        required
                    />
                </div>

                <div className="mb-3">
                    <label htmlFor="maxPageInput" className="form-label">
                        Max Page：
                    </label>
                    <input
                        type="number"
                        id="maxPageInput"
                        min="1"
                        className="form-control"
                        value={maxPage}
                        onChange={(e) => setMaxPage(e.target.value)}
                        required
                    />
                </div>

                <button type="submit" className="btn btn-primary">
                    送出爬蟲任務
                </button>
            </form>

            {message && <p className="mt-3">{message}</p>}
        </div>
    );
}
