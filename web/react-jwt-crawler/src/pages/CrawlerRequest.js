import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { getToken, logout, isAuthenticated, createCrawlRequest } from '../services/auth';
import axios from 'axios';

export default function CrawlerRequest() {
    const [mode, setMode] = useState('');
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
            await createCrawlRequest({ mode, keyword, maxPage, token });
            setMessage('✅ 任務已成功送出！');
        } catch (error) {
            setMessage('❌ 任務送出失敗，請稍後再試');
        }
    };

    return (
        <div className="container py-5">
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2 className="fw-bold">建立爬蟲任務</h2>
                <button
                    className="btn btn-outline-danger"
                    onClick={() => {
                        logout();
                        navigate('/login');
                    }}
                >
                    登出
                </button>
            </div>

            <div className="card shadow-sm">
                <div className="card-body">
                    <form onSubmit={handleSubmit}>
                        <div className="mb-3">
                            <label className="form-label">來源平台 (Mode)</label>
                            <select
                                className="form-select"
                                value={mode}
                                onChange={(e) => setMode(e.target.value)}
                                required
                            >
                                <option value="">請選擇平台</option>
                                <option value="momo">momo</option>
                                <option value="pchome">PChome</option>
                            </select>
                        </div>

                        <div className="mb-3">
                            <label className="form-label">搜尋關鍵字 (Keyword)</label>
                            <input
                                className="form-control"
                                value={keyword}
                                onChange={(e) => setKeyword(e.target.value)}
                                required
                                placeholder="例如：iPhone, Switch, 電視"
                            />
                        </div>

                        <div className="mb-3">
                            <label className="form-label">最大頁數 (Max Page)</label>
                            <input
                                className="form-control"
                                type="number"
                                min="1"
                                value={maxPage}
                                onChange={(e) => setMaxPage(e.target.value)}
                                required
                            />
                        </div>

                        <div className="d-grid">
                            <button type="submit" className="btn btn-primary">
                                🚀 送出爬蟲任務
                            </button>
                        </div>
                    </form>

                    {message && <div className="alert alert-info mt-4 text-center">{message}</div>}
                </div>
            </div>

            <div className="text-center mt-4">
                <Link to="/tasks" className="btn btn-outline-secondary">
                    📋 查看所有爬蟲任務
                </Link>
                <Link to="/product-history" className="btn btn-outline-info">
                    📋 查詢歷史價格
                </Link>
            </div>
        </div>
    );
}
