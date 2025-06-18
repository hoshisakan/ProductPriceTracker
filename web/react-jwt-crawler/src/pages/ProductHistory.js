import React, { useState } from 'react';
import { getToken, logout, isAuthenticated } from '../services/auth';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';
import { Line } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
} from 'chart.js';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend);

export default function ProductHistory() {
    const [taskId, setTaskId] = useState('');
    const [history, setHistory] = useState(null);
    const [message, setMessage] = useState('');
    const navigate = useNavigate();

    React.useEffect(() => {
        if (!isAuthenticated()) {
            navigate('/login');
        }
    }, [navigate]);

    const handleGetHistory = async (e) => {
        e.preventDefault();
        setHistory(null);
        setMessage('');
        const token = getToken();
        try {
            const res = await axios.post(
                'http://localhost/api/producthistory/get-history',
                { taskId },
                { headers: { Authorization: `Bearer ${token}` } }
            );
            const values = res.data && res.data.$values ? res.data.$values : [];
            setHistory(values);
            if (values.length === 0) {
                setMessage('查無歷史價格資料');
            }
        } catch (error) {
            setMessage('❌ 查詢失敗，請確認 TaskId 是否正確');
        }
    };

    // 準備圖表資料（多產品分組，每個產品一條線）
    let chartData = null;
    if (history && history.length > 0) {
        // 依產品ID分組
        const groupByProduct = {};
        history.forEach((item) => {
            if (!groupByProduct[item.productId]) groupByProduct[item.productId] = [];
            groupByProduct[item.productId].push(item);
        });

        // 取得所有時間點（去重排序）
        const allDates = Array.from(new Set(history.map((item) => new Date(item.capturedAt).toISOString()))).sort();

        chartData = {
            labels: allDates.map((date) => new Date(date).toLocaleString()),
            datasets: Object.entries(groupByProduct).map(([productId, items], idx) => {
                // 依照時間排序
                const sorted = [...items].sort((a, b) => new Date(a.capturedAt) - new Date(b.capturedAt));
                // 依照所有時間點對齊資料
                const priceMap = {};
                sorted.forEach((item) => {
                    priceMap[new Date(item.capturedAt).toISOString()] = item.price;
                });
                return {
                    label: `產品ID: ${productId}`,
                    data: allDates.map((date) => priceMap[date] ?? null),
                    borderColor: `hsl(${(idx * 60) % 360}, 70%, 50%)`,
                    backgroundColor: `hsla(${(idx * 60) % 360}, 70%, 50%, 0.2)`,
                    tension: 0.2,
                };
            }),
        };
    }

    return (
        <div className="container py-5">
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2 className="fw-bold">商品爬蟲任務</h2>
                {/* 回首頁連結 */}
                <Link to="/crawlerrequest" className="btn btn-outline-primary">
                    回首頁
                </Link>
            </div>
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2 className="fw-bold">查詢任務歷史價格</h2>
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
                    <form onSubmit={handleGetHistory} className="row g-2 align-items-center">
                        <div className="col-auto">
                            <input
                                className="form-control"
                                placeholder="請輸入 TaskId"
                                value={taskId}
                                onChange={(e) => setTaskId(e.target.value)}
                                required
                            />
                        </div>
                        <div className="col-auto">
                            <button type="submit" className="btn btn-success">
                                查詢歷史價格
                            </button>
                        </div>
                    </form>
                    {message && <div className="alert alert-info mt-4 text-center">{message}</div>}
                    {history && Array.isArray(history) && chartData && (
                        <div className="mt-3">
                            <h6>價格變化圖：</h6>
                            <Line data={chartData} />
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
