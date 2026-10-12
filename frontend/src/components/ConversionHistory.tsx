import React, { useState, useEffect, useCallback } from 'react';
import { Search, Download, Eye, RotateCcw, Calendar, FileText } from 'lucide-react';
import { HistoryItem, PaginatedResult } from '../types';
import { HistoryDetailModal } from './HistoryDetailModal';

export const ConversionHistory: React.FC = () => {
  const [items, setItems] = useState<HistoryItem[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filterDate, setFilterDate] = useState('');
  const [appliedDate, setAppliedDate] = useState('');
  const [loading, setLoading] = useState(false);
  const [detailModalId, setDetailModalId] = useState<string | null>(null);

  const fetchHistory = useCallback(async (targetPage: number, dateQuery?: string) => {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.append('page', targetPage.toString());
      params.append('pageSize', '10');
      if (dateQuery) {
        params.append('date', dateQuery);
      }

      const res = await fetch(`/api/converter/history?${params.toString()}`);
      if (!res.ok) throw new Error('Gagal mengambil riwayat');

      const data: PaginatedResult<HistoryItem> = await res.json();
      setItems(data.items);
      setTotalCount(data.totalCount);
      setPage(data.page);
      setTotalPages(data.totalPages || 1);
    } catch (err) {
      console.error('Error fetching history:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchHistory(page, appliedDate);
  }, [fetchHistory, page, appliedDate]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setAppliedDate(filterDate);
    setPage(1);
  };

  const handleResetFilter = () => {
    setFilterDate('');
    setAppliedDate('');
    setPage(1);
  };

  const formatDate = (isoString: string) => {
    try {
      const dt = new Date(isoString);
      return new Intl.DateTimeFormat('id-ID', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      }).format(dt);
    } catch {
      return isoString;
    }
  };

  return (
    <div className="content-card">
      <div className="card-header-section">
        <h2 className="section-title">Riwayat Konversi</h2>
        <p className="section-subtitle">Daftar riwayat konversi file Excel ke XML.</p>
      </div>

      {/* Date Filter Bar */}
      <form onSubmit={handleSearch} className="history-filter-row">
        <div className="date-input-wrapper">
          <Calendar size={18} color="#64748b" />
          <input
            type="date"
            value={filterDate}
            onChange={(e) => setFilterDate(e.target.value)}
            placeholder="Pilih Tanggal"
          />
        </div>

        <button type="submit" className="btn-primary" style={{ padding: '8px 18px', fontSize: '0.85rem' }}>
          <Search size={16} />
          <span>Cari</span>
        </button>

        {appliedDate && (
          <button
            type="button"
            className="btn-secondary"
            onClick={handleResetFilter}
            style={{ padding: '8px 14px', fontSize: '0.85rem' }}
          >
            <RotateCcw size={15} />
            <span>Reset</span>
          </button>
        )}
      </form>

      {/* Table Container */}
      <div className="data-table-container">
        <table className="custom-table">
          <thead>
            <tr>
              <th style={{ width: '50px' }}>No</th>
              <th>Tanggal & Waktu</th>
              <th>Nama File</th>
              <th>Skema</th>
              <th>Status</th>
              <th>Ukuran File</th>
              <th style={{ textAlign: 'right' }}>Aksi</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={7} style={{ textAlign: 'center', padding: '36px', color: '#64748b' }}>
                  Memuat data riwayat...
                </td>
              </tr>
            ) : items.length === 0 ? (
              <tr>
                <td colSpan={7} style={{ textAlign: 'center', padding: '36px', color: '#64748b' }}>
                  Belum ada riwayat konversi
                  {appliedDate ? ` untuk tanggal ${appliedDate}` : ''}.
                </td>
              </tr>
            ) : (
              items.map((item, idx) => {
                const rowNumber = (page - 1) * 10 + idx + 1;
                const isSuccess = item.status === 'Berhasil';

                return (
                  <tr key={item.id}>
                    <td>{rowNumber}</td>
                    <td>{formatDate(item.createdAt)}</td>
                    <td>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <FileText size={16} color="#0066cc" />
                        <span style={{ fontWeight: 500 }}>{item.displayFileName}</span>
                      </div>
                    </td>
                    <td>
                      <span className="schema-badge">
                        {item.schemaType === 'SOT_PRODUCTION' ? 'SOT Production' : 'SOT L-PUMA'}
                      </span>
                    </td>
                    <td>
                      <span className={`status-badge ${isSuccess ? 'badge-success' : 'badge-danger'}`}>
                        {item.status}
                      </span>
                    </td>
                    <td>{(item.fileSizeBytes / 1024).toFixed(0)} KB</td>
                    <td style={{ textAlign: 'right', whiteSpace: 'nowrap' }}>
                      {isSuccess && (
                        <a
                          href={item.downloadUrl}
                          target="_blank"
                          rel="noreferrer"
                          className="table-btn-download"
                        >
                          <Download size={14} />
                          <span>Download</span>
                        </a>
                      )}
                      <button
                        type="button"
                        className="table-btn-detail"
                        onClick={() => setDetailModalId(item.id)}
                      >
                        <Eye size={14} />
                        <span>Lihat Detail</span>
                      </button>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>

        {/* Pagination */}
        <div className="pagination-row">
          <div>
            Menampilkan {items.length} dari {totalCount} data
          </div>

          <div className="pagination-controls">
            <button
              type="button"
              className="page-btn"
              disabled={page <= 1}
              onClick={() => setPage((p) => Math.max(1, p - 1))}
            >
              &lt;
            </button>

            {Array.from({ length: totalPages }, (_, i) => i + 1).map((pNum) => (
              <button
                key={pNum}
                type="button"
                className={`page-btn ${page === pNum ? 'active' : ''}`}
                onClick={() => setPage(pNum)}
              >
                {pNum}
              </button>
            ))}

            <button
              type="button"
              className="page-btn"
              disabled={page >= totalPages}
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            >
              &gt;
            </button>
          </div>
        </div>
      </div>

      {/* History Detail Modal */}
      <HistoryDetailModal
        id={detailModalId}
        isOpen={!!detailModalId}
        onClose={() => setDetailModalId(null)}
      />
    </div>
  );
};
