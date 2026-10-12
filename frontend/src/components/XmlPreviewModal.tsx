import React, { useState } from 'react';
import { X, Copy, Check, Download } from 'lucide-react';

interface XmlPreviewModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  xmlContent: string;
  downloadUrl?: string;
  fileName?: string;
}

export const XmlPreviewModal: React.FC<XmlPreviewModalProps> = ({
  isOpen,
  onClose,
  title,
  xmlContent,
  downloadUrl,
  fileName,
}) => {
  const [copied, setCopied] = useState(false);

  if (!isOpen) return null;

  const handleCopy = () => {
    navigator.clipboard.writeText(xmlContent);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleDownload = () => {
    if (downloadUrl) {
      window.open(downloadUrl, '_blank');
    } else {
      const blob = new Blob([xmlContent], { type: 'application/xml' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName || 'converted_output.xml';
      a.click();
      URL.revokeObjectURL(url);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-container" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3 className="modal-title">{title}</h3>
          <button type="button" className="modal-close-btn" onClick={onClose}>
            <X size={20} />
          </button>
        </div>

        <div className="modal-body">
          <pre className="xml-code-block">{xmlContent || 'Tidak ada konten XML.'}</pre>
        </div>

        <div className="modal-footer">
          <button type="button" className="btn-secondary" onClick={handleCopy}>
            {copied ? <Check size={16} color="#10b981" /> : <Copy size={16} />}
            <span>{copied ? 'Tersalin!' : 'Salin XML'}</span>
          </button>
          <button type="button" className="btn-primary" onClick={handleDownload}>
            <Download size={16} />
            <span>Unduh XML</span>
          </button>
        </div>
      </div>
    </div>
  );
};
