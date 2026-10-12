import React, { useState, useEffect } from 'react';
import { X, Download, Copy, Check, AlertCircle } from 'lucide-react';
import { ConversionResponse } from '../types';

interface HistoryDetailModalProps {
  id: string | null;
  isOpen: boolean;
  onClose: () => void;
}

export const HistoryDetailModal: React.FC<HistoryDetailModalProps> = ({ id, isOpen, onClose }) => {
  const [data, setData] = useState<ConversionResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    if (!isOpen || !id) {
      setData(null);
      return;
    }

    setLoading(true);
    fetch(`/api/converter/history/${id}`)
      .then((res) => {
        if (!res.ok) throw new Error('Gagal memuat detail');
        return res.json();
      })
      .then((resData: ConversionResponse) => {
        setData(resData);
      })
      .catch((err) => {
        console.error(err);
      })
      .finally(() => {
        setLoading(false);
      });
  }, [id, isOpen]);

  if (!isOpen) return null;

  const handleCopy = () => {
    if (data?.xmlContent) {
      navigator.clipboard.writeText(data.xmlContent);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-container" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div>
            <h3 className="modal-title">Detail Riwayat Konversi</h3>
            <p className="section-subtitle">{data?.originalFileName || 'Memuat...'}</p>
          </div>
          <button type="button" className="modal-close-btn" onClick={onClose}>
            <X size={20} />
          </button>
        </div>

        <div className="modal-body">
          {loading ? (
            <div style={{ textAlign: 'center', padding: '40px 0', color: '#64748b' }}>
              Memuat data...
            </div>
          ) : data ? (
            <div>
              <div
                style={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))',
                  gap: '12px',
                  marginBottom: '20px',
                }}
              >
                <div style={{ background: '#f8fafc', padding: '10px 14px', borderRadius: '8px' }}>
                  <div style={{ fontSize: '0.75rem', color: '#64748b' }}>Skema</div>
                  <div style={{ fontWeight: '600', fontSize: '0.85rem' }}>
                    {data.schemaType === 'SOT_PRODUCTION' ? 'SOT Production' : 'SOT L-PUMA'}
                  </div>
                </div>

                <div style={{ background: '#f8fafc', padding: '10px 14px', borderRadius: '8px' }}>
                  <div style={{ fontSize: '0.75rem', color: '#64748b' }}>Status</div>
                  <span
                    className={`status-badge ${
                      data.status === 'Berhasil' ? 'badge-success' : 'badge-danger'
                    }`}
                  >
                    {data.status}
                  </span>
                </div>

                <div style={{ background: '#f8fafc', padding: '10px 14px', borderRadius: '8px' }}>
                  <div style={{ fontSize: '0.75rem', color: '#64748b' }}>Jumlah Baris</div>
                  <div style={{ fontWeight: '600', fontSize: '0.85rem' }}>
                    {data.rowCount} baris
                  </div>
                </div>

                <div style={{ background: '#f8fafc', padding: '10px 14px', borderRadius: '8px' }}>
                  <div style={{ fontSize: '0.75rem', color: '#64748b' }}>Ukuran File</div>
                  <div style={{ fontWeight: '600', fontSize: '0.85rem' }}>
                    {(data.fileSizeBytes / 1024).toFixed(1)} KB
                  </div>
                </div>
              </div>

              {data.errorMessage && (
                <div
                  style={{
                    background: '#fef2f2',
                    border: '1px solid #fecaca',
                    borderRadius: '8px',
                    padding: '12px 16px',
                    color: '#991b1b',
                    fontSize: '0.85rem',
                    marginBottom: '16px',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '10px',
                  }}
                >
                  <AlertCircle size={18} />
                  <span>{data.errorMessage}</span>
                </div>
              )}

              {data.xmlContent ? (
                <div>
                  <div
                    style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      marginBottom: '8px',
                    }}
                  >
                    <span style={{ fontSize: '0.85rem', fontWeight: 600 }}>Hasil XML</span>
                    <button type="button" className="btn-secondary" onClick={handleCopy} style={{ padding: '4px 10px', fontSize: '0.78rem' }}>
                      {copied ? <Check size={14} color="#10b981" /> : <Copy size={14} />}
                      <span>{copied ? 'Tersalin' : 'Salin'}</span>
                    </button>
                  </div>
                  <pre className="xml-code-block">{data.xmlContent}</pre>
                </div>
              ) : null}
            </div>
          ) : (
            <div style={{ textAlign: 'center', color: '#ef4444' }}>Gagal memuat detail riwayat.</div>
          )}
        </div>

        <div className="modal-footer">
          {data?.status === 'Berhasil' && (
            <a
              href={`/api/converter/download/${data.id}`}
              target="_blank"
              rel="noreferrer"
              className="btn-primary"
              style={{ textDecoration: 'none' }}
            >
              <Download size={16} />
              <span>Unduh XML</span>
            </a>
          )}
          <button type="button" className="btn-secondary" onClick={onClose}>
            Tutup
          </button>
        </div>
      </div>
    </div>
  );
};
