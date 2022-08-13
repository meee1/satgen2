using System;

namespace Racelogic.Maths;

public class Geometry
{
	public static Point? GetCentrePointOfCurve(Point pointOnCurve, double velocitykmh, double yawRateInDegreePerSecond, double headingInDegree, out double radius)
	{
		double num = velocitykmh * 1000.0 / 3600.0;
		double num2 = yawRateInDegreePerSecond * Math.PI / 180.0;
		if (Math.Abs(num2) < 1E-07)
		{
			radius = double.NaN;
			return null;
		}
		radius = Math.Abs(num / num2);
		if (headingInDegree > 180.0)
		{
			headingInDegree -= 360.0;
		}
		else if (headingInDegree < -180.0)
		{
			headingInDegree += 360.0;
		}
		double num3 = headingInDegree * Math.PI / 180.0;
		num3 = ((!(yawRateInDegreePerSecond > 0.0)) ? (num3 + Math.PI / 2.0) : (num3 - Math.PI / 2.0));
		double x = pointOnCurve.X - radius * Math.Sin(num3);
		double y = pointOnCurve.Y - radius * Math.Cos(num3);
		return new Point(x, y);
	}

	public static Point GetNextPointInCurve(Point pointOnCurve, Point? centrePointOfCurve, double? radius, double velocitykmh, double yawRateInDegreePerSecond, double headingInDegree, double nextPointIntervalInSeconds)
	{
		if (Math.Abs(yawRateInDegreePerSecond) < 1E-07 || !centrePointOfCurve.HasValue)
		{
			double num = velocitykmh * Math.Sin(headingInDegree * Math.PI / 180.0);
			double num2 = velocitykmh * Math.Cos(headingInDegree * Math.PI / 180.0);
			double num3 = num * 1000.0 / 3600.0 * nextPointIntervalInSeconds;
			double num4 = num2 * 1000.0 / 3600.0 * nextPointIntervalInSeconds;
			double x = pointOnCurve.X + num3;
			double y = pointOnCurve.Y + num4;
			return new Point(x, y);
		}
		double num5 = pointOnCurve.X - centrePointOfCurve.Value.X;
		double num6 = pointOnCurve.Y - centrePointOfCurve.Value.Y;
		double num7 = yawRateInDegreePerSecond * Math.PI / 180.0;
		double num8 = Math.Atan2(Math.Abs(num6), Math.Abs(num5));
		double num9 = ((num5 > 0.0 && num6 < 0.0) ? (num8 + Math.PI / 2.0) : ((num5 < 0.0 && num6 < 0.0) ? (4.71238898038469 - num8) : ((num5 < 0.0 && num6 > 0.0) ? (4.71238898038469 + num8) : ((num5 == 0.0 && num6 > 0.0) ? 0.0 : ((num5 == 0.0 && num6 < 0.0) ? Math.PI : ((num6 == 0.0 && num5 > 0.0) ? (Math.PI / 2.0) : ((num6 != 0.0 || !(num5 < 0.0)) ? (Math.PI / 2.0 - num8) : 4.71238898038469)))))));
		double num10 = num9 + num7 * nextPointIntervalInSeconds;
		if (!radius.HasValue)
		{
			radius = Math.Sqrt((pointOnCurve.X - centrePointOfCurve.Value.X) * (pointOnCurve.X - centrePointOfCurve.Value.X) + (pointOnCurve.Y - centrePointOfCurve.Value.Y) * (pointOnCurve.Y - centrePointOfCurve.Value.Y));
		}
		double x2 = centrePointOfCurve.Value.X + radius.Value * Math.Sin(num10);
		double y2 = centrePointOfCurve.Value.Y + radius.Value * Math.Cos(num10);
		return new Point(x2, y2);
	}

	public static double? GetDistanceSquareBetweenPointAndLineSegment(Point p1, Point l1, Point l2, bool extendLineToGetPerpendicularIntersection)
	{
		double num = l2.X - l1.X;
		double num2 = l2.Y - l1.Y;
		double num3 = num * num + num2 * num2;
		double num4 = ((p1.X - l1.X) * num + (p1.Y - l1.Y) * num2) / num3;
		if (!extendLineToGetPerpendicularIntersection && (num4 > 1.0 || num4 < 0.0))
		{
			if (num4 > 1.0)
			{
				num4 = 1.0;
			}
			else if (num4 < 0.0)
			{
				num4 = 0.0;
			}
		}
		double num5 = l1.X + num4 * num;
		double num6 = l1.Y + num4 * num2;
		return (num5 - p1.X) * (num5 - p1.X) + (num6 - p1.Y) * (num6 - p1.Y);
	}

	public static double GetDistanceSquareBetweenPointAndLineSegment(Point p1, Point l1, Point l2, bool extendLineToGetPerpendicularIntersection, out Point intersectionPoint)
	{
		bool isPerpendicularIntersection;
		return GetDistanceSquareBetweenPointAndLineSegment(p1, l1, l2, extendLineToGetPerpendicularIntersection, out intersectionPoint, out isPerpendicularIntersection);
	}

	public static double GetDistanceSquareBetweenPointAndLineSegment(Point p1, Point l1, Point l2, bool extendLineToGetPerpendicularIntersection, out Point intersectionPoint, out bool isPerpendicularIntersection)
	{
		double num = l2.X - l1.X;
		double num2 = l2.Y - l1.Y;
		double num3 = l2.Z - l1.Z;
		double num4 = num * num + num2 * num2;
		double num5 = ((p1.X - l1.X) * num + (p1.Y - l1.Y) * num2) / num4;
		isPerpendicularIntersection = extendLineToGetPerpendicularIntersection || (num5 >= 0.0 && num5 <= 1.0);
		if (!extendLineToGetPerpendicularIntersection && (num5 > 1.0 || num5 < 0.0))
		{
			if (num5 > 1.0)
			{
				num5 = 1.0;
			}
			else if (num5 < 0.0)
			{
				num5 = 0.0;
			}
		}
		intersectionPoint = new Point(l1.X + num5 * num, l1.Y + num5 * num2, l1.Z + num5 * num3);
		return (intersectionPoint.X - p1.X) * (intersectionPoint.X - p1.X) + (intersectionPoint.Y - p1.Y) * (intersectionPoint.Y - p1.Y);
	}

	public static bool IsLineIntersectsCircle(Point circlecentre, double radius, Point point1, Point point2, out Point intersection1, out Point intersection2)
	{
		double num = point2.X - point1.X;
		double num2 = point2.Y - point1.Y;
		double num3 = num * num + num2 * num2;
		double num4 = 2.0 * (num * (point1.X - circlecentre.X) + num2 * (point1.Y - circlecentre.Y));
		double num5 = (point1.X - circlecentre.X) * (point1.X - circlecentre.X) + (point1.Y - circlecentre.Y) * (point1.Y - circlecentre.Y) - radius * radius;
		double num6 = num4 * num4 - 4.0 * num3 * num5;
		if (num3 <= 1E-07 || num6 < 0.0)
		{
			intersection1 = new Point(double.NaN, double.NaN);
			intersection2 = new Point(double.NaN, double.NaN);
			return false;
		}
		double num7;
		if (num6 == 0.0)
		{
			num7 = (0.0 - num4) / (2.0 * num3);
			intersection1 = new Point(point1.X + num7 * num, point1.Y + num7 * num2);
			intersection2 = new Point(double.NaN, double.NaN);
			return true;
		}
		num7 = (float)((0.0 - num4 + Math.Sqrt(num6)) / (2.0 * num3));
		intersection1 = new Point(point1.X + num7 * num, point1.Y + num7 * num2);
		num7 = (float)((0.0 - num4 - Math.Sqrt(num6)) / (2.0 * num3));
		intersection2 = new Point(point1.X + num7 * num, point1.Y + num7 * num2);
		return true;
	}

	public static double GetAverageOfAngle(double angle1InDegree, double angle2InDegree)
	{
		if (Math.Abs(angle2InDegree - angle1InDegree) <= 180.0)
		{
			return (angle1InDegree + angle2InDegree) / 2.0;
		}
		double num = ((angle1InDegree < 180.0) ? angle1InDegree : (360.0 - angle1InDegree));
		double num2 = ((angle2InDegree < 180.0) ? angle2InDegree : (360.0 - angle2InDegree));
		double num3 = (num + num2) / 2.0;
		if (angle1InDegree <= 180.0 && num3 <= angle1InDegree)
		{
			return angle1InDegree - num3;
		}
		if (angle2InDegree <= 180.0 && num3 <= angle2InDegree)
		{
			return angle2InDegree - num3;
		}
		if (angle1InDegree > 180.0 && angle1InDegree + num3 < 360.0)
		{
			return angle1InDegree + num3;
		}
		return angle2InDegree + num3;
	}

	public static void RotatePoint(ref Point pointToRotate, Point centrePoint, double AngleInDegree)
	{
		double num = Math.Sin(AngleInDegree * Math.PI / 180.0);
		double num2 = Math.Sqrt(1.0 - num * num) * (double)((!(Math.Abs(AngleInDegree) > 90.0) || !(Math.Abs(AngleInDegree) < 270.0)) ? 1 : (-1));
		double num3 = pointToRotate.X - centrePoint.X;
		double num4 = pointToRotate.Y - centrePoint.Y;
		double num5 = num3 * num2 - num4 * num;
		double num6 = num3 * num + num4 * num2;
		pointToRotate.X = num5 + centrePoint.X;
		pointToRotate.Y = num6 + centrePoint.Y;
	}

	public static bool IsIntersecting(Point Line1PointA, Point Line1PointB, Point Line2PointA, Point Line2PointB, out Point? pointOfIntersection)
	{
		double num = (Line1PointA.X - Line1PointB.X) * (Line2PointA.Y - Line2PointB.Y) - (Line1PointA.Y - Line1PointB.Y) * (Line2PointA.X - Line2PointB.X);
		if (num == 0.0)
		{
			pointOfIntersection = null;
			return false;
		}
		int side = GetSide(Line1PointA, Line1PointB, Line2PointA);
		int side2 = GetSide(Line1PointA, Line1PointB, Line2PointB);
		int side3 = GetSide(Line2PointA, Line2PointB, Line1PointA);
		int side4 = GetSide(Line2PointA, Line2PointB, Line1PointB);
		if (side3 != side4 && side != side2)
		{
			double num2 = (Line1PointA.X * Line1PointB.Y - Line1PointA.Y * Line1PointB.X) * (Line2PointA.X - Line2PointB.X) - (Line1PointA.X - Line1PointB.X) * (Line2PointA.X * Line2PointB.Y - Line2PointA.Y * Line2PointB.X);
			double num3 = (Line1PointA.X * Line1PointB.Y - Line1PointA.Y * Line1PointB.X) * (Line2PointA.Y - Line2PointB.Y) - (Line1PointA.Y - Line1PointB.Y) * (Line2PointA.X * Line2PointB.Y - Line2PointA.Y * Line2PointB.X);
			pointOfIntersection = new Point(num2 / num, num3 / num);
			return true;
		}
		pointOfIntersection = null;
		return false;
	}

	public static int GetSide(Point LinePointA, Point LinePointB, Point PointToCheck)
	{
		if ((LinePointB.Y - LinePointA.Y) * (PointToCheck.X - LinePointA.X) - (LinePointB.X - LinePointA.X) * (PointToCheck.Y - LinePointA.Y) > 0.0)
		{
			return 1;
		}
		return 2;
	}
}
