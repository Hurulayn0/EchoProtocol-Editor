from pathlib import Path
from zipfile import ZipFile
import hashlib
root=Path('.').resolve()
evidence=root/'Verification/Results/2026-10-09/Immersion'
fixtures=(evidence/'package-fixtures.txt').read_text(encoding='utf-8-sig').splitlines()
source=Path(next(line.split(': ',1)[1] for line in fixtures if line.startswith('Source fixture:')))
player=Path(next(line.split(': ',1)[1] for line in fixtures if line.startswith('Player fixture:')))
checks=[]
for name,fixture,prefixes in (
    ('EchoProtocol_Unity_Source.zip',source,('Assets/','Packages/','ProjectSettings/')),
    ('EchoProtocol_Gelistirilmis_Demo_2026-10-09.zip',player,None)):
    count=0
    with ZipFile(root/'Deliverables'/name) as archive:
        assert len(archive.namelist())==len(set(archive.namelist())), 'Duplicate entries'
        for entry in archive.infolist():
            assert '..' not in Path(entry.filename).parts and not entry.filename.startswith('/'), entry.filename
            if entry.is_dir(): continue
            if prefixes and not entry.filename.startswith(prefixes): continue
            assert hashlib.sha256(archive.read(entry)).digest()==hashlib.sha256((fixture/entry.filename).read_bytes()).digest(), entry.filename
            count+=1
        if prefixes:
            assert not any(n.startswith(('Library/','Temp/','Verification/Tools/','Verification/Backups/','Builds/')) or n.endswith('.log') for n in archive.namelist()), 'Unwanted source payload'
    checks.append(f'PASS: final {name}: {count} files match the separately imported/tested package payload')
assert hashlib.sha256((root/'Deliverables/EchoProtocol_Arkadas_Test_2026-10-09.zip').read_bytes()).hexdigest()=='3a640c2978baa5a903139fe44264d6dfa561f26dcbb4fd83ca38f6c4589eae56', 'Previous friend ZIP changed'
checks.append('PASS: previous 157-check friend ZIP preserved with original SHA256')
with ZipFile(root/'Deliverables/EchoProtocol_Gelistirilmis_Demo_2026-10-09.zip') as a:
    for n in ('BASLA_BURADAN.txt','DENEME_REHBERI.txt','EchoProtocol/OYNA_TR.txt'):
        t=a.read(n).decode('utf-8-sig')
        assert 'rezon' in t.lower() or 'Rezon' in t, n
        assert '202' in t or n=='BASLA_BURADAN.txt', n
    assert hashlib.sha256(a.read('EchoProtocol/UnityPlayer.dll')).hexdigest()=='00d986fa021b98163dcd6134e0c274f489c6c893a4db6a3fee05c776713e33e2'
checks.append('PASS: updated instructions and official patched player DLL present')
(evidence/'final-payload-audit.txt').write_text('\n'.join(checks)+'\n',encoding='utf-8-sig')
print('\n'.join(checks))
