import React, { useState } from 'react';
import api from '../utils/api';
import { Line } from 'react-chartjs-2';
import { Chart as ChartJS, LineElement, CategoryScale, LinearScale, PointElement, Tooltip, Legend } from 'chart.js';
import { Container, Form, InputGroup, Button, Table, Alert, Spinner } from 'react-bootstrap';

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
        <Container className="mt-4">
            <h2>📈 商品價格歷史圖表</h2>

            <Form onSubmit={fetchHistory} className="mb-4">
                <InputGroup>
                    <Form.Control
                        type="text"
                        placeholder="輸入 taskId（例如 Task-20250618-xxxx）"
                        value={taskId}
                        onChange={(e) => setTaskId(e.target.value)}
                        required
                    />
                    <Button type="submit" variant="primary">
                        查詢
                    </Button>
                </InputGroup>
            </Form>

            {loading && <Spinner animation="border" variant="primary" className="mb-3" />}

            {error && <Alert variant="danger">{error}</Alert>}

            {history.length > 0 && (
                <>
                    <Line data={chartData} options={chartOptions} className="mb-4" />

                    <Table bordered hover responsive>
                        <thead>
                            <tr>
                                <th>名稱</th>
                                <th>價格</th>
                                <th>庫存</th>
                                <th>時間</th>
                            </tr>
                        </thead>
                        <tbody>
                            {history.map((item, idx) => (
                                <tr key={idx}>
                                    <td>{item.product.productName}</td>
                                    <td>{item.price} 元</td>
                                    <td>{item.stock || '未知'}</td>
                                    <td>{new Date(item.timestamp || item.capturedAt).toLocaleString()}</td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                </>
            )}
        </Container>
    );
};

export default PriceHistoryPage;
