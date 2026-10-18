using UnityEngine;

/*
 * Builds the dice tower and its tray out of scaled cubes (BoxColliders only) under this transform. Everything is
 * driven by the fields below, so the tower is tuned by changing geometry numbers and choosing Rebuild Tower from the
 * component's context menu, never by changing code.
 *
 * Rules of thumb that keep dice flowing: every opening a die passes through - the gap between a baffle's free edge
 * and the opposite wall, the height under a baffle, the exit mouth - must be clear of 1-2 die widths, because two dice
 * wedge across anything narrower than about three. Widths that are a whole number of dice are just as bad: a row of
 * exactly-fitting dice locks between the side walls, so the shaft is 3.5 dice wide. Baffles need to be steeper than the
 * dice's friction angle, and the exit ramp needs about 30 degrees or cube dice stop on it.
 *
 * Layout in local space: the shaft is centered on the origin with its front wall facing -Z. Baffles alternate between
 * the back and front walls, the lowest one hanging from the front so dice land at the back of the exit ramp. The ramp
 * carries them through the opening at the bottom of the front wall into the tray, which extends toward -Z.
 */
public class DiceTowerBuilder : MonoBehaviour
{
    [Header("Shaft")]
    [SerializeField] float shaftWidth = 4.9f;
    [SerializeField] float shaftDepth = 8f;
    [SerializeField] float wallThickness = 1f;
    [SerializeField] float exitHeight = 5f;

    [Header("Baffles")]
    [SerializeField, Min(1)] int baffleCount = 3;
    [SerializeField] float lowestBaffleHeight = 12.2f;
    [SerializeField] float baffleSpacing = 5f;
    [SerializeField, Range(20f, 50f)] float baffleAngle = 42f;
    [SerializeField, Range(0.4f, 0.8f)] float baffleCoverage = 0.53f;
    [SerializeField] float baffleThickness = 0.5f;
    [SerializeField] float headroom = 2.5f;

    [Header("Exit ramp")]
    [SerializeField] float rampHighY = 6f;
    [SerializeField] float rampLowY = 0.05f;
    [SerializeField, Tooltip("How far the ramp reaches past the front wall into the tray.")] float rampOverhang = 1.5f;

    [Header("Funnel")]
    [SerializeField] float funnelHeight = 3f;
    [SerializeField] float funnelFlare = 3f;

    [Header("Tray")]
    [SerializeField] Vector2 traySize = new Vector2(13f, 11f);
    [SerializeField] float trayWallHeight = 2f;

    [Header("Look and feel")]
    [SerializeField] Material wallMaterial;
    [SerializeField] Material glassMaterial;
    [SerializeField] Material trayMaterial;
    [SerializeField] PhysicsMaterial towerPhysics;
    [SerializeField] PhysicsMaterial trayPhysics;
    [SerializeField] string geometryLayer = "TowerGeometry";

    [Header("Outputs")]
    [SerializeField] BoxCollider trayZone;
    [SerializeField] Transform trayDropPoint;
    [SerializeField] Transform mouthPoint;

    public BoxCollider TrayZone => trayZone;
    public Transform TrayDropPoint => trayDropPoint;
    public Transform MouthPoint => mouthPoint;

    float HalfWidth => shaftWidth * 0.5f;
    float HalfDepth => shaftDepth * 0.5f;
    public float ShaftTop => lowestBaffleHeight + (baffleCount - 1) * baffleSpacing + headroom;
    public float MouthTop => ShaftTop + funnelHeight;

    Transform geometry;
    int layer;

    [ContextMenu("Rebuild Tower")]
    public void Rebuild()
    {
        Transform old = transform.Find("Geometry");
        if (old != null) DestroyImmediate(old.gameObject);

        geometry = new GameObject("Geometry").transform;
        geometry.SetParent(transform, false);
        layer = LayerMask.NameToLayer(geometryLayer);
        if (layer < 0) layer = 0;

        BuildShaft();
        BuildBaffles();
        BuildRamp();
        BuildFunnel();
        BuildTray();
        BuildMarkers();
    }

    void BuildShaft()
    {
        float t = wallThickness;
        float top = ShaftTop;

        Box("Wall_Back", new Vector3(0f, top * 0.5f, HalfDepth + t * 0.5f), new Vector3(shaftWidth + 2f * t, top, t), wallMaterial, towerPhysics);
        Box("Wall_Left", new Vector3(-(HalfWidth + t * 0.5f), top * 0.5f, 0f), new Vector3(t, top, shaftDepth + 2f * t), wallMaterial, towerPhysics);
        Box("Wall_Right", new Vector3(HalfWidth + t * 0.5f, top * 0.5f, 0f), new Vector3(t, top, shaftDepth + 2f * t), wallMaterial, towerPhysics);

        // The front wall stops above the exit so dice can leave; it is glass so the tumble stays visible.
        float frontHeight = top - exitHeight;
        Box("Wall_Front", new Vector3(0f, exitHeight + frontHeight * 0.5f, -(HalfDepth + t * 0.5f)), new Vector3(shaftWidth + 2f * t, frontHeight, t), glassMaterial, towerPhysics);
    }

    void BuildBaffles()
    {
        float reach = baffleCoverage * shaftDepth;
        float drop = reach * Mathf.Tan(baffleAngle * Mathf.Deg2Rad);

        for (int i = 0; i < baffleCount; i++)
        {
            // i = 0 is the lowest baffle, which hangs from the front wall.
            bool fromFront = i % 2 == 0;
            float side = fromFront ? -1f : 1f;
            float topY = lowestBaffleHeight + i * baffleSpacing;

            Vector3 wallEdge = new Vector3(0f, topY, side * HalfDepth);
            Vector3 freeEdge = new Vector3(0f, topY - drop, side * (HalfDepth - reach));
            Slab($"Baffle_{i}", wallEdge, freeEdge, shaftWidth + wallThickness, baffleThickness, wallThickness * 0.5f, wallMaterial, towerPhysics);
        }
    }

    void BuildRamp()
    {
        Vector3 high = new Vector3(0f, rampHighY, HalfDepth);
        Vector3 low = new Vector3(0f, rampLowY, -(HalfDepth + wallThickness + rampOverhang));
        Slab("Exit_Ramp", high, low, shaftWidth + wallThickness, 0.5f, wallThickness * 0.5f, wallMaterial, towerPhysics);
    }

    void BuildFunnel()
    {
        float top = ShaftTop;
        float t = wallThickness * 0.5f;
        float spanX = shaftWidth + 2f * funnelFlare + 2f * wallThickness;
        float spanZ = shaftDepth + 2f * funnelFlare + 2f * wallThickness;

        Slab("Funnel_Back", new Vector3(0f, top, HalfDepth), new Vector3(0f, top + funnelHeight, HalfDepth + funnelFlare), spanX, t, 0f, wallMaterial, towerPhysics);
        Slab("Funnel_Front", new Vector3(0f, top, -HalfDepth), new Vector3(0f, top + funnelHeight, -(HalfDepth + funnelFlare)), spanX, t, 0f, glassMaterial, towerPhysics);
        Slab("Funnel_Left", new Vector3(-HalfWidth, top, 0f), new Vector3(-(HalfWidth + funnelFlare), top + funnelHeight, 0f), spanZ, t, 0f, wallMaterial, towerPhysics);
        Slab("Funnel_Right", new Vector3(HalfWidth, top, 0f), new Vector3(HalfWidth + funnelFlare, top + funnelHeight, 0f), spanZ, t, 0f, wallMaterial, towerPhysics);
    }

    void BuildTray()
    {
        float t = wallThickness;
        float trayBack = -(HalfDepth + t);
        float trayFront = trayBack - traySize.y;
        float halfTray = traySize.x * 0.5f;
        float h = trayWallHeight;

        // One floor slab under both the tray and the tower.
        float floorBack = HalfDepth + t;
        float floorFront = trayFront - t;
        Box("Tray_Floor", new Vector3(0f, -0.5f, (floorBack + floorFront) * 0.5f), new Vector3(traySize.x + 2f * t, 1f, floorBack - floorFront), trayMaterial, trayPhysics);

        Box("Tray_Wall_Left", new Vector3(-(halfTray + t * 0.5f), h * 0.5f, (trayBack + trayFront) * 0.5f), new Vector3(t, h, traySize.y + 2f * t), wallMaterial, towerPhysics);
        Box("Tray_Wall_Right", new Vector3(halfTray + t * 0.5f, h * 0.5f, (trayBack + trayFront) * 0.5f), new Vector3(t, h, traySize.y + 2f * t), wallMaterial, towerPhysics);
        Box("Tray_Wall_Front", new Vector3(0f, h * 0.5f, trayFront - t * 0.5f), new Vector3(traySize.x + 2f * t, h, t), wallMaterial, towerPhysics);

        // The tower's front wall closes the middle of the tray's back edge; these close the rest.
        float towerHalf = HalfWidth + t;
        float sideSpan = halfTray - towerHalf;
        if (sideSpan > 0f)
        {
            float x = towerHalf + sideSpan * 0.5f;
            Box("Tray_Wall_BackLeft", new Vector3(-x, h * 0.5f, trayBack + t * 0.5f), new Vector3(sideSpan, h, t), wallMaterial, towerPhysics);
            Box("Tray_Wall_BackRight", new Vector3(x, h * 0.5f, trayBack + t * 0.5f), new Vector3(sideSpan, h, t), wallMaterial, towerPhysics);
        }

        GameObject zone = new GameObject("TrayZone");
        zone.transform.SetParent(geometry, false);
        zone.transform.localPosition = new Vector3(0f, h, (trayBack + trayFront) * 0.5f);
        zone.layer = LayerMask.NameToLayer("Ignore Raycast");
        trayZone = zone.AddComponent<BoxCollider>();
        trayZone.isTrigger = true;
        trayZone.size = new Vector3(traySize.x, h * 2f, traySize.y);
    }

    void BuildMarkers()
    {
        float trayBack = -(HalfDepth + wallThickness);
        trayDropPoint = Marker("TrayDropPoint", new Vector3(0f, trayWallHeight + 3f, trayBack - traySize.y * 0.5f));
        mouthPoint = Marker("MouthPoint", new Vector3(0f, ShaftTop, 0f));
    }

    Transform Marker(string name, Vector3 localPosition)
    {
        Transform marker = new GameObject(name).transform;
        marker.SetParent(geometry, false);
        marker.localPosition = localPosition;
        return marker;
    }

    GameObject Box(string name, Vector3 localCenter, Vector3 size, Material material, PhysicsMaterial physics)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.layer = layer;
        box.isStatic = true;
        box.transform.SetParent(geometry, false);
        box.transform.localPosition = localCenter;
        box.transform.localScale = size;
        MeshRenderer meshRenderer = box.GetComponent<MeshRenderer>();
        if (material != null) meshRenderer.sharedMaterial = material;
        if (material != null && material == glassMaterial) meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        box.GetComponent<BoxCollider>().sharedMaterial = physics;
        return box;
    }

    // A slab whose upper surface runs from edgeA to edgeB and spans `width` across the horizontal axis perpendicular
    // to that run. extendA pushes the slab past edgeA (into the wall it hangs from) so no gap is left at the joint.
    void Slab(string name, Vector3 edgeA, Vector3 edgeB, float width, float thickness, float extendA, Material material, PhysicsMaterial physics)
    {
        Vector3 along = (edgeB - edgeA).normalized;
        Vector3 start = edgeA - along * extendA;
        Vector3 across = Vector3.Cross(Vector3.up, new Vector3(along.x, 0f, along.z)).normalized;
        Vector3 normal = Vector3.Cross(along, across);
        if (normal.y < 0f) normal = -normal;

        float length = Vector3.Distance(start, edgeB);
        Vector3 center = (start + edgeB) * 0.5f - normal * (thickness * 0.5f);

        GameObject slab = Box(name, center, new Vector3(width, thickness, length), material, physics);
        slab.transform.localRotation = Quaternion.LookRotation(along, normal);
    }
}
