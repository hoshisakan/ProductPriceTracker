import React, { useState, useRef } from 'react';
import api from '../utils/api';
import { Line } from 'react-chartjs-2';
import { Chart as ChartJS, LineElement, CategoryScale, LinearScale, PointElement, Tooltip, Legend } from 'chart.js';
import { Container, Form, InputGroup, Button, Table, Alert, Spinner } from 'react-bootstrap';
import html2canvas from 'html2canvas';
import jsPDF from 'jspdf';

ChartJS.register(LineElement, CategoryScale, LinearScale, PointElement, Tooltip, Legend);

const PriceHistoryPage = () => {
    const [taskId, setTaskId] = useState('');
    const [history, setHistory] = useState([]);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const chartRef = useRef(null);
    const tableRef = useRef(null);

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

    const exportToPDF = async () => {
        if (!chartRef.current || !tableRef.current) {
            alert('沒有可匯出的內容');
            return;
        }

        const pdf = new jsPDF('p', 'mm', 'a4');
        const pdfWidth = pdf.internal.pageSize.getWidth();

        // 1. 圖表轉圖片 (Chart.js 提供的 toBase64Image)
        const chartImage = chartRef.current.toBase64Image();

        // 計算圖片顯示大小
        const chartWidth = pdfWidth - 20;
        // ChartJS 實例物件沒有 height/width 屬性，這邊先固定高度
        const chartHeight = 80;

        pdf.addImage(chartImage, 'PNG', 10, 10, chartWidth, chartHeight);

        // 2. 表格轉圖片 (html2canvas)
        const canvas = await html2canvas(tableRef.current, {
            scale: 2,
            useCORS: true,
            backgroundColor: '#fff',
        });
        const imgData = canvas.toDataURL('image/png');

        const yPosition = 10 + chartHeight + 10;
        const imgProps = pdf.getImageProperties(imgData);
        const pdfHeight = (imgProps.height * pdfWidth) / imgProps.width;

        pdf.addImage(imgData, 'PNG', 10, yPosition, pdfWidth - 20, pdfHeight);

        pdf.save(`價格歷史_${taskId}.pdf`);
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
                    <Button variant="outline-danger" className="mb-3" onClick={exportToPDF}>
                        匯出為 PDF
                    </Button>

                    <Line ref={chartRef} data={chartData} options={chartOptions} className="mb-4" />

                    <div ref={tableRef}>
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
                                        <td>{item.product?.productName || '無名稱'}</td>
                                        <td>{item.price} 元</td>
                                        <td>{item.stock || '未知'}</td>
                                        <td>{new Date(item.timestamp || item.capturedAt).toLocaleString()}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </Table>
                    </div>
                </>
            )}
        </Container>
    );
};

export default PriceHistoryPage;
