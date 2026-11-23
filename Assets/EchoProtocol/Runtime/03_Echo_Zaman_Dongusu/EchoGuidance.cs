using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private bool echoTeaching, pendingEchoRecord;
        private int replacementEchoSlot = -1;
        private float planPreviewTime;

        private void FinishEchoTeaching()
        {
            echoTeaching = false; planning = false; SetCursor(!testing);
            Notify("Hareketin ve E etkileşimlerin kaydediliyor. R ile bu kaydı Echo olarak oynat.");
        }
        private void OpenEchoRecordReview()
        {
            TogglePlanning(); pendingEchoRecord = true;
        }
        private void ChooseEchoReplacement(int index)
        {
            if (!planning || index < 0 || index >= recordings.Count) return;
            if (pendingEchoRecord) { CommitLoop(index); return; }
            TogglePlanning(); replacementEchoSlot = index;
            Notify("Sonraki R bu denemeyi Echo " + (index + 1) + " yerine kaydeder. Diğer Echo korunur. T kayıtları koruyup döngüyü başlatır.");
        }
        private void CommitReviewedNewEcho()
        {
            if (sampleActive && planning && pendingEchoRecord && recordings.Count < MaxEchoes) CommitLoop();
        }
        private void CancelEchoRecordReview()
        {
            if (pendingEchoRecord && clock >= ActiveLoopLength) RetryLevel();
            else TogglePlanning();
        }

    }
}
