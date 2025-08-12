using CellsLinker.Runtime;

public class LevelGraph {
    public LevelGraphNode Root { get; private set; }
    public int Count { get; private set; }

    public LevelGraph(LevelGraphNode root) {
        Root = root;
        Count++;
    }

    public int NewId() => Count++;
}