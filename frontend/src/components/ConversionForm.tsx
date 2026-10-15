import React, { useState, useRef } from 'react';
import { UploadCloud, FileSpreadsheet, Trash2, Settings, Download, Eye, CheckCircle2, AlertTriangle, RefreshCw } from 'lucide-react';
import { SchemaType, ConversionResponse } from '../types';
import { XmlPreviewModal } from './XmlPreviewModal';

interface ConversionFormProps {
  onConversionSuccess: () => void;
}

export const ConversionForm: React.FC<ConversionFormProps> = ({ onConversionSuccess }) => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [schemaType, setSchemaType] = useState<SchemaType>('SOT_PRODUCTION');
  const [customFileName, setCustomFileName] = useState('');
  const [isDragging, setIsDragging] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [result, setResult] = useState<ConversionResponse | null>(null);
  const [previewOpen, setPreviewOpen] = useState(false);

  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      validateAndSetFile(e.dataTransfer.files[0]);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      validateAndSetFile(e.target.files[0]);
    }
  };

  const validateAndSetFile = (file: File) => {
    setErrorMessage(null);
    setResult(null);
    const ext = file.name.split('.').pop()?.toLowerCase();
    if (ext !== 'xlsx' && ext !== 'xls') {
      setErrorMessage('Format file tidak didukung. Harap pilih file .xlsx atau .xls.');
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      setErrorMessage('Ukuran file melebihi batas maksimal 10 MB.');
      return;
    }
    setSelectedFile(file);
  };

  const handleRemoveFile = () => {
    setSelectedFile(null);
    setErrorMessage(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const handleConvert = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedFile) {
      setErrorMessage('Silakan pilih file Excel terlebih dahulu.');
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);

    const formData = new FormData();
    formData.append('file', selectedFile);
    formData.append('schemaType', schemaType);
    if (customFileName.trim()) {
      formData.append('customFileName', customFileName.trim());
    }

    try {
      const response = await fetch('/api/converter/convert', {
        method: 'POST',
        body: formData,
      });

      const data: ConversionResponse = await response.json();

      if (!response.ok) {
        throw new Error(data.errorMessage || 'Konversi gagal diproses oleh server.');
      }

      setResult(data);
      onConversionSuccess();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Terjadi kesalahan saat memproses konversi.';
      setErrorMessage(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleReset = () => {
    setSelectedFile(null);
    setCustomFileName('');
    setResult(null);
    setErrorMessage(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  return (
    <div className="content-card">
      <div className="card-header-section">
        <h2 className="section-title">Konversi Excel ke XML</h2>
        <p className="section-subtitle">
          Unggah file Excel Anda, lalu klik tombol konversi untuk menghasilkan file XML.
        </p>
      </div>

      <form onSubmit={handleConvert}>
        {/* Schema Type Selector */}
        <div className="schema-selector-container">
          <label className="selector-label">Pilih Jenis Skema Data Migas</label>
          <div className="schema-pills">
            <div
              className={`schema-pill ${schemaType === 'SOT_PRODUCTION' ? 'active' : ''}`}
              onClick={() => setSchemaType('SOT_PRODUCTION')}
            >
              <div className="schema-pill-title">
                <span>1. SOT Production (ProdML)</span>
                {schemaType === 'SOT_PRODUCTION' && <CheckCircle2 size={16} color="#0066cc" />}
              </div>
              <p className="schema-pill-desc">
                Struktur volume produksi, fasilitas, aliran fluida, dan neraca kargo.
              </p>
            </div>

            <div
              className={`schema-pill ${schemaType === 'SOT_LPUMA' ? 'active' : ''}`}
              onClick={() => setSchemaType('SOT_LPUMA')}
            >
              <div className="schema-pill-title">
                <span>2. SOT L-PUMA</span>
                {schemaType === 'SOT_LPUMA' && <CheckCircle2 size={16} color="#0066cc" />}
              </div>
              <p className="schema-pill-desc">
                Struktur data Loss Production Opportunity, event downtime, shutdown, dan sumur OFF.
              </p>
            </div>
          </div>
        </div>

        {/* Dropzone */}
        <div
          className={`dropzone-box ${isDragging ? 'dragging' : ''}`}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
          onClick={() => fileInputRef.current?.click()}
        >
          <input
            ref={fileInputRef}
            type="file"
            accept=".xlsx, .xls"
            style={{ display: 'none' }}
            onChange={handleFileChange}
          />

          <div className="dropzone-icon-circle">
            <UploadCloud size={28} />
          </div>

          <p className="dropzone-main-text">
            <span className="dropzone-highlight">Pilih file Excel</span> atau drag & drop file di sini
          </p>
          <p className="dropzone-sub-text">
            Format yang didukung: .xlsx, .xls &nbsp;|&nbsp; Maksimal ukuran: 10 MB
          </p>

          {selectedFile && (
            <div
              className="selected-file-display"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="file-info-group">
                <FileSpreadsheet size={24} color="#0066cc" />
                <div style={{ textAlign: 'left' }}>
                  <div className="file-name-text">{selectedFile.name}</div>
                  <div className="file-size-text">
                    {(selectedFile.size / 1024).toFixed(1)} KB
                  </div>
                </div>
              </div>
              <button
                type="button"
                className="file-remove-btn"
                title="Hapus file"
                onClick={handleRemoveFile}
              >
                <Trash2 size={16} />
              </button>
            </div>
          )}
        </div>

        {/* Custom File Name Input */}
        <div className="form-input-group">
          <label className="input-label" htmlFor="customFileName">
            Nama File (opsional)
          </label>
          <input
            id="customFileName"
            type="text"
            className="text-input"
            placeholder="Contoh: SOT_2026-04-09"
            value={customFileName}
            onChange={(e) => setCustomFileName(e.target.value)}
          />
        </div>

        {errorMessage && (
          <div
            style={{
              background: '#fef2f2',
              border: '1px solid #fecaca',
              padding: '12px 16px',
              borderRadius: '8px',
              color: '#991b1b',
              fontSize: '0.88rem',
              marginBottom: '20px',
              display: 'flex',
              alignItems: 'center',
              gap: '10px',
            }}
          >
            <AlertTriangle size={18} />
            <span>{errorMessage}</span>
          </div>
        )}

        {/* Submit Button */}
        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
          <button
            type="submit"
            className="btn-primary"
            disabled={!selectedFile || isSubmitting}
          >
            {isSubmitting ? (
              <>
                <RefreshCw size={18} className="animate-spin" />
                <span>Memproses Konversi...</span>
              </>
            ) : (
              <>
                <Settings size={18} />
                <span>Konversi ke XML</span>
              </>
            )}
          </button>
        </div>
      </form>

      {/* Success Result Banner */}
      {result && result.status === 'Berhasil' && (
        <div className="conversion-result-banner">
          <div className="result-header">
            <div className="result-status-title">
              <CheckCircle2 size={24} color="#166534" />
              <span>Konversi Berhasil Diselesaikan!</span>
            </div>
            <button type="button" className="btn-secondary" onClick={handleReset}>
              Konversi File Lain
            </button>
          </div>

          <div className="result-grid-stats">
            <div className="stat-item">
              <div className="stat-label">File Asli</div>
              <div className="stat-val">{result.originalFileName}</div>
            </div>
            <div className="stat-item">
              <div className="stat-label">Nama Hasil</div>
              <div className="stat-val">{result.customFileName || result.originalFileName}.xml</div>
            </div>
            <div className="stat-item">
              <div className="stat-label">Skema Data</div>
              <div className="stat-val">
                {result.schemaType === 'SOT_PRODUCTION' ? 'SOT Production' : 'SOT L-PUMA'}
              </div>
            </div>
            <div className="stat-item">
              <div className="stat-label">Data Terproses</div>
              <div className="stat-val">{result.rowCount} Baris</div>
            </div>
          </div>

          {result.warnings && result.warnings.length > 0 && (
            <div
              style={{
                background: '#fffbeb',
                border: '1px solid #fde68a',
                borderRadius: '8px',
                padding: '12px 16px',
                marginTop: '16px',
                marginBottom: '16px',
                color: '#92400e',
                fontSize: '0.85rem',
              }}
            >
              <div style={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '6px' }}>
                <AlertTriangle size={16} color="#d97706" />
                <span>Peringatan Validasi ({result.warnings.length} Catatan):</span>
              </div>
              <ul style={{ paddingLeft: '20px', margin: 0, maxHeight: '160px', overflowY: 'auto' }}>
                {result.warnings.map((w, idx) => (
                  <li key={idx} style={{ marginBottom: '4px' }}>{w}</li>
                ))}
              </ul>
            </div>
          )}

          <div className="result-action-row">
            <a
              href={`/api/converter/download/${result.id}`}
              target="_blank"
              rel="noreferrer"
              className="btn-primary"
              style={{ textDecoration: 'none' }}
            >
              <Download size={18} />
              <span>Unduh Hasil XML</span>
            </a>

            <button
              type="button"
              className="btn-secondary"
              onClick={() => setPreviewOpen(true)}
            >
              <Eye size={18} />
              <span>Lihat Preview XML</span>
            </button>
          </div>
        </div>
      )}

      {/* Modal XML Preview */}
      {result && (
        <XmlPreviewModal
          isOpen={previewOpen}
          onClose={() => setPreviewOpen(false)}
          title={`Preview XML: ${result.customFileName || result.originalFileName}.xml`}
          xmlContent={result.xmlContent}
          downloadUrl={`/api/converter/download/${result.id}`}
          fileName={`${result.customFileName || result.originalFileName}.xml`}
        />
      )}
    </div>
  );
};
