import React, { useState } from 'react';
import { Download, FileSpreadsheet, CheckCircle2, BookOpen, Layers, Filter } from 'lucide-react';

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

        {/* Dukungan AutoFilter & Grouping (Row Visibility) */}
        <div style={{ background: '#f8fafc', border: '1px solid #cbd5e1', borderLeft: '4px solid #0066cc', borderRadius: '12px', padding: '20px', marginBottom: '32px' }}>
          <h4 style={{ fontSize: '0.95rem', fontWeight: 700, color: '#0f4c81', marginBottom: '8px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Filter size={18} color="#0066cc" />
            <span>Dukungan Excel Filter & Grouping (Row Visibility)</span>
          </h4>
          <p style={{ fontSize: '0.85rem', color: '#334155', lineHeight: 1.6, marginBottom: '10px' }}>
            Aplikasi secara otomatis membaca status tampilan baris (<em>row visibility</em>) pada file Excel Anda (.xlsx):
          </p>
          <ul style={{ fontSize: '0.82rem', color: '#475569', lineHeight: 1.6, paddingLeft: '20px', margin: 0, display: 'flex', flexDirection: 'column', gap: '4px' }}>
            <li><strong>AutoFilter:</strong> Baris yang tersembunyi karena filter di Excel tidak akan diikutsertakan ke dalam XML.</li>
            <li><strong>Grouping / Outline:</strong> Grup baris yang di-<em>collapse</em> (tanda minus [-] ditutup menjadi [+]) otomatis dilewati dan tidak diekspor.</li>
            <li><strong>Hide Rows Manual:</strong> Baris yang disembunyikan secara manual (klik kanan &gt; Hide) juga otomatis dilewati.</li>
          </ul>
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
                <tr><td>1</td><td><span className="code-tag">prodVolumeName</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:name</td></tr>
                <tr><td>2</td><td><span className="code-tag">prodVolumeOperatorName</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:operator/prodml:name</td></tr>
                <tr><td>3</td><td><span className="code-tag">facilityName</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:name</td></tr>
                <tr><td>4</td><td><span className="code-tag">facilityKind</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:name/@kind</td></tr>
                <tr><td>5</td><td><span className="code-tag">facilityNamingSystem</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:namingSystem</td></tr>
                <tr><td>6</td><td><span className="code-tag">facilityParent1</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:facilityParent1</td></tr>
                <tr><td>7</td><td><span className="code-tag">facilityParent2</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:facilityParent2</td></tr>
                <tr><td>8</td><td><span className="code-tag">contextFacility</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:contextFacility</td></tr>
                <tr><td>9</td><td><span className="code-tag">flowName</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:name</td></tr>
                <tr><td>10</td><td><span className="code-tag">flowKind</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:kind</td></tr>
                <tr><td>11</td><td><span className="code-tag">flowDirection</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:direction</td></tr>
                <tr><td>12</td><td><span className="code-tag">flowQualifier</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:qualifier</td></tr>
                <tr><td>13</td><td><span className="code-tag">productName</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:name</td></tr>
                <tr><td>14</td><td><span className="code-tag">productKind</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:kind</td></tr>
                <tr><td>15</td><td><span className="code-tag">periodKind</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:kind</td></tr>
                <tr><td>16</td><td><span className="code-tag">periodDate</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:date</td></tr>
                <tr><td>17</td><td><span className="code-tag">periodVolumeValue</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:volume</td></tr>
                <tr><td>18</td><td><span className="code-tag">periodVolumeValueUom</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:volume/@uom</td></tr>
                <tr><td>19</td><td><span className="code-tag">periodBlnceSetKind</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:kind</td></tr>
                <tr><td>20</td><td><span className="code-tag">BlnceSetCargoNumber</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:cargoNumber</td></tr>
                <tr><td>21</td><td><span className="code-tag">BlnceSetCargoBatchNumber</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:cargoBatchNumber</td></tr>
                <tr><td>22</td><td><span className="code-tag">BlnceSetShipper</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:shipper</td></tr>
                <tr><td>23</td><td><span className="code-tag">BlnceSetBuyer</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:buyer</td></tr>
                <tr><td>24</td><td><span className="code-tag">BlnceSetDestinationName</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:destination/prodml:name</td></tr>
                <tr><td>25</td><td><span className="code-tag">BlnceSetOwner</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:balanceDetail/prodml:owner</td></tr>
                <tr><td>26</td><td><span className="code-tag">BlnceSetEventKind</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:balanceDetail/prodml:event/prodml:kind</td></tr>
                <tr><td>27</td><td><span className="code-tag">periodBlnceSetVolValue</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:balanceDetail/prodml:volumeValue/prodml:volume</td></tr>
                <tr><td>28</td><td><span className="code-tag">periodBlnceVolValueUom</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:balanceDetail/prodml:volumeValue/prodml:volume/@uom</td></tr>
                <tr><td>29</td><td><span className="code-tag">periodCompContWork</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:balanceDetail/prodml:componentContent/prodml:work</td></tr>
                <tr><td>30</td><td><span className="code-tag">periodCompContWorkUom</span></td><td>/prodml:productVolumes/prodml:productVolume[]/prodml:facility[]/prodml:flow[]/prodml:product[]/prodml:period/prodml:balanceSet[]/prodml:balanceDetail/prodml:componentContent/prodml:work/@uom</td></tr>
              </tbody>
            </table>
          ) : (
            <table className="mapping-table">
              <thead>
                <tr>
                  <th>No</th>
                  <th>Source Data (Kolom Excel)</th>
                  <th>XML Element / Atribut</th>
                  <th>XML Path (LPO Schema)</th>
                </tr>
              </thead>
              <tbody>
                <tr><td>1</td><td><span className="code-tag">KKKS</span> / <span className="code-tag">operator</span></td><td>operator</td><td>/lossProductionOpportunity/operator</td></tr>
                <tr><td>2</td><td><span className="code-tag">WK</span> / <span className="code-tag">wilayahKerja</span></td><td>wilayahKerja</td><td>/lossProductionOpportunity/wilayahKerja</td></tr>
                <tr><td>3</td><td><span className="code-tag">FACILITY_TYPE</span> / <span className="code-tag">[Field/Area/Train/Platform/All]</span></td><td>@facilityKind</td><td>/events/eventDate/event/facility/@facilityKind</td></tr>
                <tr><td>4</td><td><span className="code-tag">FACILITY_NAME</span> / <span className="code-tag">[Field/...] Name</span></td><td>@facilityName</td><td>/events/eventDate/event/facility/@facilityName</td></tr>
                <tr><td>5</td><td><span className="code-tag">EVENT_DATE</span> / <span className="code-tag">Tanggal</span></td><td>@date</td><td>/events/eventDate/@date</td></tr>
                <tr><td>6</td><td><span className="code-tag">DAY_NUMBER</span> / <span className="code-tag">Hari ke-</span></td><td>days</td><td>/events/eventDate/event/facility/days</td></tr>
                <tr><td>7</td><td><span className="code-tag">START_TIME</span> / <span className="code-tag">Start Time</span></td><td>startTime</td><td>/events/eventDate/event/facility/startTime (ISO-8601)</td></tr>
                <tr><td>8</td><td><span className="code-tag">END_TIME</span> / <span className="code-tag">End Time</span></td><td>endTime</td><td>/events/eventDate/event/facility/endTime (ISO-8601)</td></tr>
                <tr><td>9</td><td><span className="code-tag">EVENT_ID</span> / <span className="code-tag">Event ID</span></td><td>@eventId</td><td>/events/eventDate/event/@eventId</td></tr>
                <tr><td>10</td><td><span className="code-tag">CATEGORY</span> / <span className="code-tag">Kategori</span></td><td>category</td><td>/events/eventDate/event/facility/category (Enum XSD)</td></tr>
                <tr><td>11</td><td><span className="code-tag">SYSTEM_SOURCE_LEVEL_1</span> / <span className="code-tag">System Source Level 1</span></td><td>systemSourceLevel1</td><td>/events/eventDate/event/facility/systemSourceLevel1 (Enum XSD)</td></tr>
                <tr><td>12</td><td><span className="code-tag">SYSTEM_SOURCE_LEVEL_2</span> / <span className="code-tag">System Source Level 2</span></td><td>systemSourceLevel2</td><td>/events/eventDate/event/facility/systemSourceLevel2 (Enum XSD)</td></tr>
                <tr><td>13</td><td><span className="code-tag">SYSTEM_CAUSE_LEVEL_1</span> / <span className="code-tag">System Cause Level1</span></td><td>systemCauseLevel1</td><td>/events/eventDate/event/facility/systemCauseLevel1 (Enum XSD)</td></tr>
                <tr><td>14</td><td><span className="code-tag">SYSTEM_CAUSE_LEVEL_2</span> / <span className="code-tag">System Cause Level2</span></td><td>systemCauseLevel2</td><td>/events/eventDate/event/facility/systemCauseLevel2 (Enum XSD)</td></tr>
                <tr><td>15</td><td><span className="code-tag">ACTIVITY_DESCRIPTION</span> / <span className="code-tag">Kejadian/Kegiatan</span></td><td>eventsActivities</td><td>/events/eventDate/event/facility/eventsActivities</td></tr>
                <tr><td>16</td><td><span className="code-tag">UPDATE_PROGRESS</span> / <span className="code-tag">Update Progress</span></td><td>updateProgress</td><td>/events/eventDate/event/facility/updateProgress (Opsional)</td></tr>
                <tr><td>17</td><td><span className="code-tag">TOTAL_WELL_OFF</span> / <span className="code-tag">Jumlah Sumur OFF</span></td><td>totalWellOFF</td><td>/events/eventDate/event/facility/totalWellOFF (Opsional)</td></tr>
                <tr><td>18</td><td><span className="code-tag">ESTIMATED_FINISH_DATE</span> / <span className="code-tag">Est. Tanggal Selesai</span></td><td>estFinishDate</td><td>/events/eventDate/event/facility/estFinishDate (Opsional)</td></tr>
                <tr><td>19</td><td><span className="code-tag">VOLUME_OIL</span> / <span className="code-tag">Volume LPO Minyak (BBLS)</span></td><td>volumeOil</td><td>/events/eventDate/event/facility/volumeOil (Decimal)</td></tr>
                <tr><td>20</td><td><span className="code-tag">VOLUME_GAS</span> / <span className="code-tag">Volume LPO Gas (MMSCF)</span></td><td>volumeGas</td><td>/events/eventDate/event/facility/volumeGas (Decimal)</td></tr>
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
};
