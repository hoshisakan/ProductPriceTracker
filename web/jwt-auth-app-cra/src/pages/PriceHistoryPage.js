import React, { useState } from 'react';
import api from '../utils/api';
import { Line } from 'react-chartjs-2';
import { Chart as ChartJS, LineElement, CategoryScale, LinearScale, PointElement, Tooltip, Legend } from 'chart.js';

ChartJS.register(LineElement, CategoryScale, LinearScale, PointElement, Tooltip, Legend);

const PriceHistoryPage = () => {
    const [taskId, setTaskId] = useState('');
    const [history, setHistory] = useState([]);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const fetchHistory = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        setHistory([]);

        try {
            const response = await api.post('/producthistory/get-history', { taskId });
            setHistory(response.data.$values || []);
        } catch (err) {
            setError('無法取得價格歷史：' + (err.response?.data?.message || err.message));
        } finally {
            setLoading(false);
        }
    };

    const chartData = {
        labels: history.map((h) => new Date(h.timestamp || h.capturedAt).toLocaleString()),
        datasets: [
            {
                label: '價格變化',
                data: history.map((h) => h.price),
                borderColor: 'rgba(75,192,192,1)',
                fill: false,
                tension: 0.3,
            },
        ],
    };

    const chartOptions = {
        responsive: true,
        plugins: {
            legend: { position: 'top' },
        },
        scales: {
            x: {
                ticks: { maxTicksLimit: 10 },
            },
            y: {
                beginAtZero: false,
            },
        },
    };

    return (
        <div className="container mt-4">
            <h2>📈 商品價格歷史圖表</h2>

            <form onSubmit={fetchHistory} className="mb-3">
                <div className="input-group">
                    <input
                        type="text"
                        className="form-control"
                        placeholder="輸入 taskId（例如 Task-20250618-xxxx）"
                        value={taskId}
                        onChange={(e) => setTaskId(e.target.value)}
                        required
                    />
                    <button type="submit" className="btn btn-primary">
                        查詢
                    </button>
                </div>
            </form>

            {loading && <p>載入中...</p>}
            {error && <div className="alert alert-danger">{error}</div>}

            {history.length > 0 && (
                <>
                    <Line data={chartData} options={chartOptions} className="mb-4" />

                    <table className="table table-bordered">
                        <thead>
                            <tr>
                                <th>價格</th>
                                <th>庫存</th>
                                <th>時間</th>
                            </tr>
                        </thead>
                        <tbody>
                            {history.map((item, idx) => (
                                <tr key={idx}>
                                    <td>${item.price}</td>
                                    <td>{item.stock}</td>
                                    <td>{new Date(item.timestamp || item.capturedAt).toLocaleString()}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </>
            )}
        </div>
    );
};

export default PriceHistoryPage;
