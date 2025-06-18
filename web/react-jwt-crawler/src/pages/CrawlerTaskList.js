// src/pages/CrawlerTaskList.js
import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import { getToken } from '../services/auth';

export default function CrawlerTaskList() {
    const [tasks, setTasks] = useState([]);
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchTasks = async () => {
            try {
                const response = await axios.post(
                    'http://localhost/api/crawlertask/get-crawler-task',
                    {},
                    { headers: { Authorization: `Bearer ${getToken()}` } }
                );
                setTasks(response.data.$values || []);
            } catch (err) {
                setError('❌ 無法取得任務列表，請稍後再試。');
            }
        };

        fetchTasks();
    }, []);

    return (
        <div className="container py-5">
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2 className="fw-bold">商品爬蟲任務</h2>
                {/* 回首頁連結 */}
                <Link to="/crawlerrequest" className="btn btn-outline-primary">
                    回首頁
                </Link>
            </div>
            <div className="card shadow-sm">
                <div className="card-body">
                    <h3 className="mb-4 fw-bold">📋 爬蟲任務列表</h3>

                    {error && <div className="alert alert-danger">{error}</div>}

                    {tasks.length === 0 && !error ? (
                        <div className="alert alert-info text-center">尚無任務紀錄。</div>
                    ) : (
                        <div className="table-responsive">
                            <table className="table table-hover align-middle">
                                <thead className="table-dark">
                                    <tr>
                                        <th scope="col">任務編號</th>
                                        <th scope="col">來源參數</th>
                                        <th scope="col">狀態</th>
                                        <th scope="col">建立時間</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {tasks.map((task) => (
                                        <tr key={task.id}>
                                            <td className="fw-bold">{task.taskId}</td>
                                            <td>
                                                <code>{task.source}</code>
                                            </td>
                                            <td>
                                                <span
                                                    className={`badge ${
                                                        task.status === 'Received'
                                                            ? 'bg-secondary'
                                                            : task.status === 'Processing'
                                                            ? 'bg-warning text-dark'
                                                            : task.status === 'Completed'
                                                            ? 'bg-success'
                                                            : 'bg-dark'
                                                    }`}
                                                >
                                                    {task.status}
                                                </span>
                                            </td>
                                            <td>{new Date(task.createdAt).toLocaleString()}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
