using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Original code by Jason Yang - https://github.com/dci05049/Verlet-Rope-Unity
//Explained on Youtube - https://www.youtube.com/watch?v=FcnvwtyxLds
//Additions by Julia

public class LineRope : MonoBehaviour
{

    private LineRenderer lineRenderer;
    private EdgeCollider2D edgeCollider;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private float ropeSegLen = 0.25f;
    private int segmentLength = 35;
    private float lineWidth = 0.1f;

    private ContactPoint2D[] contactPoints;

    void Start()
    {
        this.lineRenderer = this.GetComponent<LineRenderer>();
        this.edgeCollider = this.GetComponent<EdgeCollider2D>();
        
        Vector3 ropeStartPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Initialize rope segments
        for (int i = 0; i < segmentLength; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= ropeSegLen;
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.DrawRope();
    }

    private void FixedUpdate()
    {
        this.Simulate();
    }

    private void Simulate()
    {
        // SIMULATION
        Vector2 forceGravity = new Vector2(0f, -1.5f);

        for (int i = 1; i < this.segmentLength; i++)
        {
            RopeSegment firstSegment = this.ropeSegments[i];
            Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += velocity;
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
            this.ropeSegments[i] = firstSegment;
        }

        //CONSTRAINTS
        for (int i = 0; i < 100; i++)
        {
            this.ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        //Attach first segment of rope to mouse position
        RopeSegment firstSegment = this.ropeSegments[0];
        firstSegment.posNow = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.ropeSegments[0] = firstSegment;

        for (int i = 0; i < this.segmentLength - 1; i++)
        {
            RopeSegment firstSeg = this.ropeSegments[i];
            RopeSegment secondSeg = this.ropeSegments[i + 1];

            
            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - this.ropeSegLen);
            Vector2 changeDir = Vector2.zero;

            //Make sure two rope segments do not get too far apart or too close together
            if (dist > ropeSegLen)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            } else if (dist < ropeSegLen)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0) //for all rope points excluding the first point (which is attached to the mouse)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                this.ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                //Julia added this - check to make sure the point isn't colliding with anything
                //And if it is, we set that value to respect the collision points
                secondSeg = CheckColliders(secondSeg);
                this.ropeSegments[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                this.ropeSegments[i + 1] = secondSeg;
            }

            
        }
    }

    //Julia's collider function
    //Basically if a rope points is near a point of contact with another collider
    //We set that rope point's bounds to not move past that contact point
    RopeSegment CheckColliders(RopeSegment firstSeg){
        Vector2 newChangeDir = Vector2.zero;
        if(contactPoints != null){
            foreach(ContactPoint2D cp in contactPoints){
                if((firstSeg.posNow - cp.point).magnitude < 0.15f){ //Gets the rope point if it is within 0.15f distance of the contact point
                    newChangeDir = cp.normal; //get the normal of the contact point to determine which side of the point the collider is on
                    if(newChangeDir.x < 0){ //The rope point is to the left of the collider point
                        if(firstSeg.posNow.x>cp.point.x){ //the rope's x value should not be GREATER than the collider point's x value otherwise it will have moved past the collider bounds
                            // Turning on these debug logs will slow the heck down in the editor. FYI.
                            // Debug.Log("rope point is on the left of the box. firstseg pos now x = "+firstSeg.posNow.x+" and cp point x is "+cp.point.x+" firstsegx is farther right than the box so we are moving it left");
                            
                            //We set the current rope point x to be the collision x - that's the MAXIMUM x it can be without entering the collider
                            firstSeg.posNow.x = cp.point.x;
                            //We set the "old" position to the same value so we do not get a big spike in velocity that makes the rope fly out
                            firstSeg.posOld.x = cp.point.x;
                        }
                    }
                    else if(newChangeDir.x > 0){ //the rope point is to the right of the collider point
                        if(firstSeg.posNow.x<cp.point.x){ //the rope's x value should not be LESS than the collider point's x value otherwise it will have moved past the collider bounds
                            // Turning on these debug logs will slow the heck down in the editor. FYI.
                            // Debug.Log("rope point is on the right of the box. firstseg pos now x = "+firstSeg.posNow.x+" and cp point x is "+cp.point.x+" firstsegx is farther left than the box so we are moving it right");
                            
                            //We set the current rope point x to be the collision x - that's the MINIMUM x it can be without entering the collider
                            firstSeg.posNow.x = cp.point.x;
                            //We set the "old" position to the same value so we do not get a big spike in velocity that makes the rope fly out
                            firstSeg.posOld.x = cp.point.x;
                        }
                    }
                    if(newChangeDir.y < 0){//the rope point is below the collider point
                        if(firstSeg.posNow.y>cp.point.y){//the rope's y value should not be GREATER than the collider point's y value otherwise it will have moved past the collider bounds
                            // Debug.Log("rope point is on the bottom of the box. firstseg pos now y = "+firstSeg.posNow.y+" and cp point y is "+cp.point.y+" firstsegY is farther up than the box so we are moving it down");
                            
                            //We set the current rope point y to be the collision y - that's the MAXIMUM y it can be without entering the collider
                            firstSeg.posNow.y = cp.point.y;
                            //We set the "old" position to the same value so we do not get a big spike in velocity that makes the rope fly out
                            firstSeg.posOld.y = cp.point.y;
                        }
                    }
                    else if(newChangeDir.y > 0){//the rope point is above the collider point
                        if(firstSeg.posNow.y<cp.point.y){//the rope's y value should not be LESS than the collider point's y value otherwise it will have moved past the collider bounds
                            // Debug.Log("rope point is on the top of the box. firstseg pos now y = "+firstSeg.posNow.y+" and cp point y is "+cp.point.y+" firstsegY is farther down than the box so we are moving it up");
                            
                            //We set the current rope point y to be the collision y - that's the MINIMUM y it can be without entering the collider
                            firstSeg.posNow.y = cp.point.y;
                            //We set the "old" position to the same value so we do not get a big spike in velocity that makes the rope fly out
                            firstSeg.posOld.y = cp.point.y;
                        }
                    }
                }
            }
        }
        return firstSeg;
    }

    private void DrawRope()
    {
        float lineWidth = this.lineWidth;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector2[] edgePositions = new Vector2[this.segmentLength];

        Vector3[] ropePositions = new Vector3[this.segmentLength];
        for (int i = 0; i < this.segmentLength; i++)
        {
            ropePositions[i] = this.ropeSegments[i].posNow;
            edgePositions[i] = this.ropeSegments[i].posNow;
            // Vector3 worldToLocal = Camera.main.transform.InverseTransformPoint(this.ropeSegments[i].posNow);
            // Debug.Log("Setting edgecollider "+i+" to "+worldToLocal);
            // edgeCollider.points[i] = new Vector2(worldToLocal.x, worldToLocal.y);
            // Debug.Log("Setting edgecollider[i] =  "+edgeCollider.points[i]);
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
        edgeCollider.points = edgePositions;
        
    }

     void OnCollisionStay2D(Collision2D collision)
    {
        //Sets all collision contact points
        contactPoints = collision.contacts;
        // Uncomment this chunk if you wish to Visualize the contact points
        // foreach (ContactPoint2D contact in collision.contacts)
        // {
        //     Debug.DrawRay(contact.point, contact.normal, Color.red);
        // }
    }
    void OnCollisionExit2D(){
        //Resets contact points to empty
        contactPoints = null;
    }


    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }
}