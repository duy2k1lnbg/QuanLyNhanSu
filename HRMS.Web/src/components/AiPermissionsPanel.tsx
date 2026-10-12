import { useCallback, useEffect, useRef, useState } from 'react';
import { Alert, Button, Input, Modal, Space, Switch, Table, Tag, Tooltip, Typography, message } from 'antd';
import { InfoCircleOutlined, ReloadOutlined, SaveOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import api from '../services/api';
import { useAppLanguage } from '../services/i18n';
import { aiCapabilityLabel, buildAiGrants, createAiDraft, hasInheritedAiDeny, isAiPermissionOn, toggleAiPermission } from '../utils/aiPermissions';
import type { AiCapability, AiDraft, AiOverview } from '../utils/aiPermissions';

type Props = { subjectId: number; isGroup: boolean; visible: boolean; onDirtyChange: (dirty: boolean) => void; onBusyChange?: (busy: boolean) => void; onSuccess?: () => void };
const { Text } = Typography;
export default function AiPermissionsPanel({ subjectId, isGroup, visible, onDirtyChange, onBusyChange, onSuccess }: Props) {
  const { t } = useAppLanguage();
  const text = (key: string) => t('aiPermissions.' + key);
  const [overview, setOverview] = useState<AiOverview>();
  const [draft, setDraft] = useState<AiDraft>({});
  const [dirty, setDirty] = useState(false);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [conflict, setConflict] = useState(false);
  const [search, setSearch] = useState('');
  const requestId = useRef(0);
  useEffect(() => { onBusyChange?.(saving); return () => onBusyChange?.(false); }, [saving, onBusyChange]);
  const markDirty = (value: boolean) => { setDirty(value); onDirtyChange(value); };
  const load = useCallback(async () => {
    const id = ++requestId.current;
    setLoading(true); setError(''); setConflict(false);
    try {
      const scope = await api.get('/admin/ai-scope', { params: { subjectType: isGroup ? 'GROUP' : 'USER', subjectId } });
      if (id !== requestId.current) return;
      const data = scope.data.data as AiOverview;
      setOverview(data); setDraft(createAiDraft(data.Capabilities));
      setDirty(false); onDirtyChange(false);
    } catch (err: unknown) {
      if (id !== requestId.current) return;
      const response = (err as { response?: { data?: { message?: string } } }).response;
      setError(response?.data?.message || t('aiPermissions.loadError'));
    } finally { if (id === requestId.current) setLoading(false); }
  }, [subjectId, isGroup, onDirtyChange, t]);
  // A hidden tab retains its draft. A new subject or modal session gets a fresh snapshot.
  useEffect(() => {
    if (visible && !overview && !loading && !error) void load();
  }, [visible, overview, loading, error, load]);
  useEffect(() => () => { requestId.current++; }, []);
  const reload = () => dirty ? Modal.confirm({ title: text('discardTitle'), content: text('discard'), onOk: load }) : void load();
  const toggle = (capability: AiCapability, enabled: boolean) => {
    setDraft(current => ({ ...current, [capability.CapabilityCode]: toggleAiPermission(capability, current[capability.CapabilityCode] ?? [], enabled) }));
    markDirty(true);
  };
  const save = async () => {
    if (!overview?.IsReady || saving || !dirty) return;
    let grants;
    try { grants = buildAiGrants(overview.Capabilities, draft); }
    catch { message.error(text('invalid')); return; }
    setSaving(true); setError('');
    try {
      await api.post('/admin/ai-scope', { SubjectType: isGroup ? 'GROUP' : 'USER', SubjectId: subjectId,
        BaseRevision: overview.CurrentRevision, Grants: grants });
      message.success(text('saved')); markDirty(false); await load(); onSuccess?.();
    } catch (err: unknown) {
      const response = (err as { response?: { status?: number; data?: { Message?: string; message?: string; ValidationErrors?: string[] } } }).response;
      setConflict(response?.status === 409);
      setError(response?.status === 409 ? text('conflict') : response?.data?.ValidationErrors?.join('\n') || response?.data?.Message || response?.data?.message || text('saveError'));
    } finally { setSaving(false); }
  };
  const columns: ColumnsType<AiCapability> = [
    { title: text('capability'), key: 'name', render: (_, c) => <Space size={8}>
      <Text>{aiCapabilityLabel(c, t)}</Text>
      {(!c.HasRequiredFunctionRight || hasInheritedAiDeny(c)) && c.IsCapabilityEnabled &&
        <Tooltip title={hasInheritedAiDeny(c) ? text('groupBlocked') : text('missingHelp')}><InfoCircleOutlined style={{ color: '#faad14' }} /></Tooltip>}
    </Space> },
    { title: text('allow'), key: 'permission', width: 150, align: 'center', render: (_, c) => !c.IsCapabilityEnabled
      ? <Text type="secondary">{text('unavailable')}</Text>
      : <Tooltip title={hasInheritedAiDeny(c) ? text('groupBlocked') : undefined}>
        <Switch aria-label={aiCapabilityLabel(c, t)} checkedChildren={text('yes')} unCheckedChildren={text('no')}
          checked={isAiPermissionOn(c, draft[c.CapabilityCode] ?? [])}
          disabled={saving || loading || !overview?.IsReady || hasInheritedAiDeny(c)}
          onChange={enabled => toggle(c, enabled)} />
      </Tooltip> },
  ];
  return <Space direction="vertical" size={12} style={{ width: '100%' }}>
    <Text type="secondary">{text('simpleHelp')}</Text>
    {error && <Alert showIcon type="error" message={error} style={{ whiteSpace: 'pre-wrap' }} />}
    {overview && !overview.IsReady && <Alert showIcon type="warning" message={overview.NotReadyReason || text('unavailable')} />}
    {overview && !overview.HasAiFeatureRight && <Alert showIcon type="warning" message={text('entryMissingSimple')} />}
    <Space wrap style={{ justifyContent: 'space-between', width: '100%' }}>
      <Input.Search value={search} onChange={e => setSearch(e.target.value)} placeholder={text('simpleSearch')} allowClear style={{ maxWidth: 320 }} />
      <Space wrap>{dirty && <Tag color="orange">{text('unsaved')}</Tag>}
        <Button icon={<ReloadOutlined />} onClick={reload} disabled={saving || loading}>{text('reload')}</Button>
        <Button type="primary" icon={<SaveOutlined />} onClick={() => void save()} loading={saving} disabled={!dirty || !overview?.IsReady || loading || conflict}>{text('save')}</Button></Space>
    </Space>
    <Table rowKey="CapabilityCode" columns={columns} loading={loading}
      dataSource={overview?.Capabilities.filter(c => (aiCapabilityLabel(c, t) + c.CapabilityCode).toLowerCase().includes(search.toLowerCase())) ?? []}
      pagination={false} size="middle" bordered scroll={{ y: 390 }} />
    <Text type="secondary">{text('scopePreserved')}</Text>
  </Space>;
}
