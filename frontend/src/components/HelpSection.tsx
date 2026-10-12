import React, { useState } from 'react';
import { Download, FileSpreadsheet, CheckCircle2, BookOpen, Layers } from 'lucide-react';

export const HelpSection: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'sot-production' | 'sot-lpuma'>('sot-production');

  return (
    <div>
      <div className="content-card">
        <div className="card-header-section">
          <h2 className="section-title">Panduan & Template Konversi Excel ke XML</h2>
          <p className="section-subtitle">
            Unduh contoh template Excel yang sudah sesuai dengan standar skema SKK Migas, atau lihat referensi pemetaan kolom.
          </p>
        </div>

        {/* Template Downloads */}
        <div className="template-cards-grid">
          {/* Card 1: SOT Production */}
          <div className="template-card">
            <div>
              <div className="template-card-header">
                <div className="template-card-icon">
                  <FileSpreadsheet size={24} />
                </div>
                <div>
                  <h3 style={{ fontSize: '1.05rem', fontWeight: 700, color: '#0f4c81' }}>
                    Template SOT Production (ProdML)
                  </h3>
                  <p style={{ fontSize: '0.8rem', color: '#64748b' }}>
                    Format data volume produksi migas harian, fasilitas, aliran fluida, dan neraca kargo.
                  </p>
                </div>
              </div>
              <ul style={{ fontSize: '0.8rem', color: '#334155', paddingLeft: '20px', marginBottom: '20px' }}>
                <li>31 kolom standar ProdML</li>
                <li>Format tanggal: YYYY-MM-DD</li>
                <li>Mendukung relasi fasilitas dan neraca kargo</li>
              </ul>
            </div>
            <a
              href="/api/converter/template/sot-production"
              download="Template_SOT_Production.xlsx"
              className="btn-primary"
              style={{ textDecoration: 'none', width: '100%' }}
            >
              <Download size={16} />
              <span>Unduh Template SOT Production (.xlsx)</span>
            </a>
          </div>

          {/* Card 2: SOT L-PUMA */}
          <div className="template-card">
            <div>
              <div className="template-card-header">
                <div className="template-card-icon" style={{ background: '#eff6ff', color: '#2563eb' }}>
                  <FileSpreadsheet size={24} />
                </div>
                <div>
                  <h3 style={{ fontSize: '1.05rem', fontWeight: 700, color: '#0f4c81' }}>
                    Template SOT L-PUMA
                  </h3>
                  <p style={{ fontSize: '0.8rem', color: '#64748b' }}>
                    Format data Loss Production Opportunity (LPO), shutdown event, sumur OFF, dan downtime.
                  </p>
                </div>
              </div>
              <ul style={{ fontSize: '0.8rem', color: '#334155', paddingLeft: '20px', marginBottom: '20px' }}>
                <li>22 kolom standar L-PUMA XML</li>
                <li>Kategori Planned & Unplanned</li>
                <li>Pelacakan volume kehilangan minyak (BBLS) & gas (MMSCF)</li>
              </ul>
            </div>
            <a
              href="/api/converter/template/sot-lpuma"
              download="Template_SOT_LPUMA.xlsx"
              className="btn-primary"
              style={{ textDecoration: 'none', width: '100%', background: 'linear-gradient(135deg, #0284c7 0%, #0369a1 100%)' }}
            >
              <Download size={16} />
              <span>Unduh Template SOT L-PUMA (.xlsx)</span>
            </a>
          </div>
        </div>

        {/* Steps Guide matching Attachment 1 */}
        <div style={{ background: '#f8fafc', border: '1px solid #e2e8f0', borderRadius: '12px', padding: '24px', marginBottom: '32px' }}>
          <h3 style={{ fontSize: '1rem', fontWeight: 700, color: '#0f4c81', marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <BookOpen size={18} />
            <span>Langkah Penggunaan Aplikasi</span>
          </h3>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px' }}>
            <div style={{ background: '#ffffff', padding: '14px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
              <div style={{ fontWeight: 700, color: '#0066cc', marginBottom: '4px' }}>Langkah 1</div>
              <p style={{ fontSize: '0.82rem', color: '#475569' }}>Upload file Excel (.xlsx / .xls) pada form utama.</p>
            </div>
            <div style={{ background: '#ffffff', padding: '14px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
              <div style={{ fontWeight: 700, color: '#0066cc', marginBottom: '4px' }}>Langkah 2</div>
              <p style={{ fontSize: '0.82rem', color: '#475569' }}>(Opsional) Isi nama file untuk memudahkan pencarian di histori.</p>
            </div>
            <div style={{ background: '#ffffff', padding: '14px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
              <div style={{ fontWeight: 700, color: '#0066cc', marginBottom: '4px' }}>Langkah 3</div>
              <p style={{ fontSize: '0.82rem', color: '#475569' }}>Klik tombol "Konversi ke XML" dan tunggu proses validasi.</p>
            </div>
            <div style={{ background: '#ffffff', padding: '14px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
              <div style={{ fontWeight: 700, color: '#0066cc', marginBottom: '4px' }}>Langkah 4</div>
              <p style={{ fontSize: '0.82rem', color: '#475569' }}>Unduh hasil XML setelah selesai atau lihat riwayat konversi.</p>
            </div>
          </div>
        </div>

        {/* Persyaratan yang harus dipenuhi matching Attachment 1 */}
        <div style={{ background: '#f0fdf4', border: '1px solid #bbf7d0', borderRadius: '12px', padding: '20px', marginBottom: '32px' }}>
          <h4 style={{ fontSize: '0.92rem', fontWeight: 700, color: '#166534', marginBottom: '10px' }}>
            Persyaratan Yang Harus Dipenuhi (Syarat Teknis Tender):
          </h4>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', fontSize: '0.82rem', color: '#15803d' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <CheckCircle2 size={16} />
              <span>Form upload, konversi, dan unduh hasil XML.</span>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <CheckCircle2 size={16} />
              <span>Histori tersimpan per hari dengan status dan waktu.</span>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <CheckCircle2 size={16} />
              <span>Peserta Tender menyerahkan link aplikasi yang dapat diakses (Docker container ready).</span>
            </div>
          </div>
        </div>

        {/* Mapping Tables Reference */}
        <div>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '14px' }}>
            <h3 style={{ fontSize: '1.05rem', fontWeight: 700, color: '#0f4c81', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Layers size={18} />
              <span>Referensi Pemetaan Kolom Excel ke Tag XML</span>
            </h3>
            <div style={{ display: 'flex', gap: '8px' }}>
              <button
                type="button"
                className={`btn-secondary ${activeTab === 'sot-production' ? 'active' : ''}`}
                style={{
                  background: activeTab === 'sot-production' ? '#0f4c81' : '#ffffff',
                  color: activeTab === 'sot-production' ? '#ffffff' : '#0f4c81',
                }}
                onClick={() => setActiveTab('sot-production')}
              >
                SOT Production (ProdML)
              </button>
              <button
                type="button"
                className={`btn-secondary ${activeTab === 'sot-lpuma' ? 'active' : ''}`}
                style={{
                  background: activeTab === 'sot-lpuma' ? '#0f4c81' : '#ffffff',
                  color: activeTab === 'sot-lpuma' ? '#ffffff' : '#0f4c81',
                }}
                onClick={() => setActiveTab('sot-lpuma')}
              >
                SOT L-PUMA (XML)
              </button>
            </div>
          </div>

          {activeTab === 'sot-production' ? (
            <table className="mapping-table">
              <thead>
                <tr>
                  <th>No</th>
                  <th>Source Data (Kolom Excel)</th>
                  <th>XML Path (ProdML Structure)</th>
                </tr>
              </thead>
              <tbody>
                <tr><td>1</td><td><span className="code-tag">prodVolumeName</span></td><td>/productVolumes/productVolume[]/name</td></tr>
                <tr><td>2</td><td><span className="code-tag">prodVolumeOperatorName</span></td><td>/productVolumes/productVolume[]/operator/name</td></tr>
                <tr><td>3</td><td><span className="code-tag">facilityName</span></td><td>/productVolumes/productVolume[]/facility[]/name</td></tr>
                <tr><td>4</td><td><span className="code-tag">facilityKind</span></td><td>/productVolumes/productVolume[]/facility[]/name/@kind</td></tr>
                <tr><td>5</td><td><span className="code-tag">facilityNamingSystem</span></td><td>/productVolumes/productVolume[]/facility[]/namingSystem</td></tr>
                <tr><td>6</td><td><span className="code-tag">facilityParent1</span></td><td>/productVolumes/productVolume[]/facility[]/facilityParent1</td></tr>
                <tr><td>7</td><td><span className="code-tag">facilityParent1Kind</span></td><td>/productVolumes/productVolume[]/facility[]/facilityParent1/@kind</td></tr>
                <tr><td>8</td><td><span className="code-tag">facilityParent2</span></td><td>/productVolumes/productVolume[]/facility[]/facilityParent2</td></tr>
                <tr><td>9</td><td><span className="code-tag">facilityParent2Kind</span></td><td>/productVolumes/productVolume[]/facility[]/facilityParent2/@kind</td></tr>
                <tr><td>10</td><td><span className="code-tag">flowName</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/name</td></tr>
                <tr><td>11</td><td><span className="code-tag">flowKind</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/kind</td></tr>
                <tr><td>12</td><td><span className="code-tag">flowDirection</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/direction</td></tr>
                <tr><td>13</td><td><span className="code-tag">flowQualifier</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/qualifier</td></tr>
                <tr><td>14</td><td><span className="code-tag">productName</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/name</td></tr>
                <tr><td>15</td><td><span className="code-tag">productKind</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/kind</td></tr>
                <tr><td>16</td><td><span className="code-tag">periodKind</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/kind</td></tr>
                <tr><td>17</td><td><span className="code-tag">periodDate</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/date</td></tr>
                <tr><td>18</td><td><span className="code-tag">periodVolumeValue</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/volume</td></tr>
                <tr><td>19</td><td><span className="code-tag">periodVolumeValueUom</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/volume/@uom</td></tr>
                <tr><td>20</td><td><span className="code-tag">periodBlnceSetKind</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/kind</td></tr>
                <tr><td>21</td><td><span className="code-tag">BlnceSetCargoNumber</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/cargoNumber</td></tr>
                <tr><td>22</td><td><span className="code-tag">BlnceSetCargoBatchNumber</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/cargoBatchNumber</td></tr>
                <tr><td>23</td><td><span className="code-tag">BlnceSetShipper</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/shipper</td></tr>
                <tr><td>24</td><td><span className="code-tag">BlnceSetBuyer</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/buyer</td></tr>
                <tr><td>25</td><td><span className="code-tag">BlnceSetDestinationName</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/destination/name</td></tr>
                <tr><td>26</td><td><span className="code-tag">BlnceSetOwner</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/balanceDetail/owner</td></tr>
                <tr><td>27</td><td><span className="code-tag">BlnceSetEventKind</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/balanceDetail/event/kind</td></tr>
                <tr><td>28</td><td><span className="code-tag">periodBlnceSetVolValue</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/balanceDetail/volumeValue/volume</td></tr>
                <tr><td>29</td><td><span className="code-tag">periodBlnceVolValueUom</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/balanceDetail/volumeValue/@uom</td></tr>
                <tr><td>30</td><td><span className="code-tag">periodCompContWork</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/balanceDetail/componentContent/work/</td></tr>
                <tr><td>31</td><td><span className="code-tag">periodCompContWorkUom</span></td><td>/productVolumes/productVolume[]/facility[]/flow[]/product[]/period/balanceSet[]/balanceDetail/componentContent/work/@uom</td></tr>
              </tbody>
            </table>
          ) : (
            <table className="mapping-table">
              <thead>
                <tr>
                  <th>No</th>
                  <th>Source Data (Kolom Excel)</th>
                  <th>XML Element</th>
                  <th>XML Path</th>
                </tr>
              </thead>
              <tbody>
                <tr><td>1</td><td><span className="code-tag">By sistem KKKS</span></td><td>documentName</td><td>/lossProductionOpportunity/documentName</td></tr>
                <tr><td>2</td><td><span className="code-tag">By sistem KKKS</span></td><td>documentDate</td><td>/lossProductionOpportunity/documentDate</td></tr>
                <tr><td>3</td><td><span className="code-tag">KKKS</span></td><td>operator</td><td>/lossProductionOpportunity/operator</td></tr>
                <tr><td>4</td><td><span className="code-tag">WK</span></td><td>wilayahKerja</td><td>/lossProductionOpportunity/wilayahKerja</td></tr>
                <tr><td>5</td><td><span className="code-tag">[Field/Area/Train/Platform/All]</span></td><td>facilityKind</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/facilityKind</td></tr>
                <tr><td>6</td><td><span className="code-tag">[Field/Area/Train/Platform/All] Name</span></td><td>facilityName</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/facilityName</td></tr>
                <tr><td>7</td><td><span className="code-tag">Tanggal</span></td><td>eventDate</td><td>/lossProductionOpportunity/events/eventDate/@date</td></tr>
                <tr><td>8</td><td><span className="code-tag">Hari ke-</span></td><td>days</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/days</td></tr>
                <tr><td>9</td><td><span className="code-tag">Start Time</span></td><td>startTime</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/startTime</td></tr>
                <tr><td>10</td><td><span className="code-tag">End Time</span></td><td>endTime</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/endTime</td></tr>
                <tr><td>11</td><td><span className="code-tag">eventID</span></td><td>shutdownID</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/shutdownID</td></tr>
                <tr><td>12</td><td><span className="code-tag">Kategori</span></td><td>category</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/category</td></tr>
                <tr><td>13</td><td><span className="code-tag">System Source Level 1</span></td><td>systemSourceLevel1</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/systemSourceLevel1</td></tr>
                <tr><td>14</td><td><span className="code-tag">System Source Level 2</span></td><td>systemSourceLevel2</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/systemSourceLevel2</td></tr>
                <tr><td>15</td><td><span className="code-tag">System Cause Level1</span></td><td>systemCauseLevel1</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/systemCauseLevel1</td></tr>
                <tr><td>16</td><td><span className="code-tag">System Cause Level2</span></td><td>systemCauseLevel2</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/systemCauseLevel2</td></tr>
                <tr><td>17</td><td><span className="code-tag">Kejadian/Kegiatan</span></td><td>eventsActivities</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/eventsActivities</td></tr>
                <tr><td>18</td><td><span className="code-tag">Update Progress</span></td><td>updateProgress</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/updateProgress</td></tr>
                <tr><td>19</td><td><span className="code-tag">Jumlah Sumur OFF</span></td><td>totalWellOFF</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/totalWellOFF</td></tr>
                <tr><td>20</td><td><span className="code-tag">Est. Tanggal Selesai</span></td><td>estFinishTime</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/estFinishTime</td></tr>
                <tr><td>21</td><td><span className="code-tag">Volume LPO Minyak (BBLS)</span></td><td>volumeOil</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/volumeOil</td></tr>
                <tr><td>22</td><td><span className="code-tag">Volume LPO Gas (MMSCF)</span></td><td>volumeGas</td><td>/lossProductionOpportunity/events/eventDate/event[1]/facility/volumeGas</td></tr>
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
};
