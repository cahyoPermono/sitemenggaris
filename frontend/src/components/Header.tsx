import React, { useMemo } from 'react';
import { FileCode2, Calendar, User } from 'lucide-react';

export const Header: React.FC = () => {
  const formattedDate = useMemo(() => {
    const now = new Date();
    return new Intl.DateTimeFormat('id-ID', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    }).format(now);
  }, []);

  return (
    <header className="app-header">
      <div className="header-brand">
        <div className="brand-icon-box">
          <FileCode2 size={22} color="#ffffff" />
        </div>
        <div>
          <h1 className="header-title">Excel to XML Converter</h1>
          <p className="header-subtitle">SKK Migas - SOT Production & L-PUMA</p>
        </div>
      </div>

      <div className="header-right">
        <div className="header-date-badge">
          <Calendar size={15} />
          <span>{formattedDate}</span>
        </div>
        <div className="header-user-avatar" title="Pengguna KKKS">
          <User size={18} color="#ffffff" />
        </div>
      </div>
    </header>
  );
};
