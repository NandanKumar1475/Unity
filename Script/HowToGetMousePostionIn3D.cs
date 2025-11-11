public class MousePositioiN3D
{
	public Camera camera;
	Ray ray = camera.screenPointToRay(input.mousePostion);
	RayCast hit;
	if(physics.RayCast(ray, out hit))
	{
		vector3 mousePostion = hit.point;
		transform.postion(mousePostion);
	}
}