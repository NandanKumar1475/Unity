public claas Vector : MonoBehaviour
{
	vector3 vect =  new Vector3(3,4,0);
	// internally formula used in this =  v = Sqrt(x^2 + y^2 + z^2)
	flaot dist = vect.magnitude ;
	float sqrtOfDistance = vect.sqrtMagnitude; //=> 25
	
	// unit vector  --> v = v/|v| -- it gives the same Direction length 1 ;
	  vect.normalized ;
	
	
	//Dot product (alignment meter)

Formula
//a · b = ax*bx + ay*by + az*bz = |a||b| cos(θ)

//Positive → pointing roughly same way

//Zero → perpendicular

//Negative → opposite

//n unity vector3.dot(a,b);
	
	
}