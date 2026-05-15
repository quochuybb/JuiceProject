using System.Collections.Generic;

public class MapNodeData 
{
    public string nodeID;
    public NodeType type; 
    
    public int layerIndex;
    public int rowIndex;

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