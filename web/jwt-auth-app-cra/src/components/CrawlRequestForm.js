import React, { useState } from 'react';
import api from '../utils/api';

const CrawlRequestForm = () => {
    const [mode, setMode] = useState('momo');
    const [keyword, setKeyword] = useState('');
    const [maxPage, setMaxPage] = useState(1);
    const [message, setMessage] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');

        if (!keyword) {
            setMessage('請輸入關鍵字');
            return;
        }

        try {
            const response = await api.post('/crawlrequests', {
                mode,
                keyword,
                maxPage: Number(maxPage),
            });
            console.log('任務建立成功:', response.data);
            setMessage('任務建立成功！');
        } catch (error) {
            setMessage('任務建立失敗：' + (error.response?.data?.message || error.message));
        }
    };

    return (
        <form onSubmit={handleSubmit} className="mb-4">
            <div className="mb-3">
                <label>模式 (mode)</label>
                <select className="form-select" value={mode} onChange={(e) => setMode(e.target.value)}>
                    <option value="momo">momo</option>
                    <option value="pchome">pchome</option>
                </select>
            </div>

            <div className="mb-3">
                <label>關鍵字 (keyword)</label>
                <input
                    type="text"
                    className="form-control"
                    value={keyword}
                    onChange={(e) => setKeyword(e.target.value)}
                    placeholder="輸入商品關鍵字"
                />
            </div>

            <div className="mb-3">
                <label>最大頁數 (maxPage)</label>
                <input
                    type="number"
                    className="form-control"
                    value={maxPage}
                    onChange={(e) => setMaxPage(e.target.value)}
                    min={1}
                    max={20}
                />
            </div>

            <button type="submit" className="btn btn-primary">
                送出爬蟲任務
            </button>

            {message && <div className="mt-3 alert alert-info">{message}</div>}
        </form>
    );
};

export default CrawlRequestForm;
