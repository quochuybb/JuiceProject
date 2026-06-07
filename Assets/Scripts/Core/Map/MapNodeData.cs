using System.Collections.Generic;
public enum NodeState
{
    Locked,
    Available,
    Completed
}
public class MapNodeData 
{
    public string nodeID;
    public NodeType type;

    public int chapterIndex;
    public int layerIndex;
    public int nodeIndexInLayer;
    public GameModeType gameMode;
    public float targetScore;

    public NodeState state = NodeState.Locked;
    public List<MapNodeData> incomingEdges = new List<MapNodeData>();
    public List<MapNodeData> outgoingEdges = new List<MapNodeData>();

    public void AddOutgoingEdge(MapNodeData targetNode)
    {
        if (!outgoingEdges.Contains(targetNode))
        {
            outgoingEdges.Add(targetNode);
        }
    }

    public void AddIncomingEdge(MapNodeData sourceNode)
    {
        if (!incomingEdges.Contains(sourceNode))
        {
            incomingEdges.Add(sourceNode);
        }
    }
}