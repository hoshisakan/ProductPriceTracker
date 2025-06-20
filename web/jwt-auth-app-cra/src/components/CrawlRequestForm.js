import React, { useState } from 'react';
import api from '../utils/api';
import { Form, Button, Alert } from 'react-bootstrap';

const CrawlRequestForm = () => {
    const [mode, setMode] = useState('momo');
    const [keyword, setKeyword] = useState('');
    const [maxPage, setMaxPage] = useState(1);
    const [message, setMessage] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');

        if (!keyword) {
            setMessage('❗ 請輸入關鍵字');
            return;
        }

        try {
            const response = await api.post('/crawlrequests', {
                mode,
                keyword,
                maxPage: Number(maxPage),
            });
            console.log('任務建立成功:', response.data);
            setMessage('✅ 任務建立成功！');
        } catch (error) {
            setMessage('❌ 任務建立失敗：' + (error.response?.data?.message || error.message));
        }
    };

    return (
        <Form onSubmit={handleSubmit} className="mb-4">
            <Form.Group className="mb-3">
                <Form.Label>模式 (mode)</Form.Label>
                <Form.Select value={mode} onChange={(e) => setMode(e.target.value)}>
                    <option value="momo">momo</option>
                    <option value="pchome">pchome</option>
                </Form.Select>
            </Form.Group>

            <Form.Group className="mb-3">
                <Form.Label>關鍵字 (keyword)</Form.Label>
                <Form.Control
                    type="text"
                    placeholder="輸入商品關鍵字"
                    value={keyword}
                    onChange={(e) => setKeyword(e.target.value)}
                />
            </Form.Group>

            <Form.Group className="mb-3">
                <Form.Label>最大頁數 (maxPage)</Form.Label>
                <Form.Control
                    type="number"
                    min={1}
                    max={20}
                    value={maxPage}
                    onChange={(e) => setMaxPage(e.target.value)}
                />
            </Form.Group>

            <Button type="submit" variant="primary">
                送出爬蟲任務
            </Button>

            {message && (
                <Alert variant="info" className="mt-3">
                    {message}
                </Alert>
            )}
        </Form>
    );
};

export default CrawlRequestForm;
