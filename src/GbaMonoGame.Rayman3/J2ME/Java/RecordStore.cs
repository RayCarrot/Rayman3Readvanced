namespace GbaMonoGame.Rayman3.J2ME;

// TODO: Implement - dummy class right now
// Replaces javax.microedition.rms.RecordStore
public class RecordStore
{
    public static RecordStore openRecordStore(string recordStoreName, bool createIfNecessary)
    {
        return new RecordStore();
    }

    public void closeRecordStore()
    {

    }

    public static string[] listRecordStores()
    {
        return [];
    }

    public byte[] getRecord(int recordId)
    {
        return null;
    }

    public void setRecord(int recordId, byte[] newData, int offset, int numBytes)
    {

    }

    public void addRecord(byte[] data, int offset, int numBytes)
    {

    }

    public int getNumRecords()
    {
        return 0;
    }

    public static void deleteRecordStore(string recordStoreName)
    {
        
    }
}