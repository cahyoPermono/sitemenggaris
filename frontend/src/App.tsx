import React, { useState } from 'react';
import { Header } from './components/Header';
import { Sidebar, ActiveTab } from './components/Sidebar';
import { ConversionForm } from './components/ConversionForm';
import { ConversionHistory } from './components/ConversionHistory';
import { HelpSection } from './components/HelpSection';

export const App: React.FC = () => {
  const [activeTab, setActiveTab] = useState<ActiveTab>('home');
  const [refreshKey, setRefreshKey] = useState(0);

  const handleConversionSuccess = () => {
    // When a conversion succeeds, increment refreshKey so history stays up-to-date
    setRefreshKey((k) => k + 1);
  };

  return (
    <div className="app-layout">
      <Header />
      <div className="app-body">
        <Sidebar activeTab={activeTab} onSelectTab={setActiveTab} />
        <main className="app-main">
          {activeTab === 'home' && (
            <ConversionForm onConversionSuccess={handleConversionSuccess} />
          )}
          {activeTab === 'history' && (
            <ConversionHistory key={refreshKey} />
          )}
          {activeTab === 'help' && (
            <HelpSection />
          )}
        </main>
      </div>
    </div>
  );
};

export default App;
