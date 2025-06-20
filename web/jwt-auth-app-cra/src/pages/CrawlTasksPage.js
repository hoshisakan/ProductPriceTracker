import React, { useEffect, useState } from 'react';
import api from '../utils/api';
import Table from 'react-bootstrap/Table';
import Container from 'react-bootstrap/Container';



const CrawlTasksPageRB = () => {
    const [tasks, setTasks] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchTasks = async () => {
            setLoading(true);
            setError('');
            try {
                const response = await api.post('/crawlertask/get-crawler-task');
                if (!response.data.$values || !Array.isArray(response.data.$values)) {
                    console.error('無效的任務資料格式:', response);
                    throw new Error('無效的任務資料格式');
                }
                setTasks(response.data.$values);
            } catch (err) {
                setError('無法取得爬蟲任務清單: ' + (err.response?.data?.message || err.message));
            } finally {
                setLoading(false);
            }
        };

        fetchTasks();
    }, []);

    return (
        <Container className="mt-4">
            <h1>爬蟲任務清單</h1>
            {loading && <p>載入中...</p>}
            {error && <div className="alert alert-danger">{error}</div>}
            {!loading && !error && tasks.length === 0 && <p>目前沒有爬蟲任務</p>}
            {!loading && tasks.length > 0 && (
                <Table striped bordered hover>
                    <thead>
                        <tr>
                            <th>任務 ID</th>
                            <th>來源 (Source)</th>
                            <th>狀態</th>
                            <th>建立時間</th>
                        </tr>
                    </thead>
                    <tbody>
                        {tasks.map((task) => (
                            <tr key={task.id}>
                                <td>{task.taskId || task.id}</td>
                                <td>{task.source || task.mode}</td>
                                <td>{task.status || task.state}</td>
                                <td>{new Date(task.createdAt || task.createdDate).toLocaleString()}</td>
                            </tr>
                        ))}
                    </tbody>
                </Table>
            )}
        </Container>
    );
}

export default CrawlTasksPageRB;