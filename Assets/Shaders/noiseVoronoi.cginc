#ifndef NOISE_VORONOI_FUNC
#define NOISE_VORONOI_FUNC
//calculate a 2D random number, used for producing voronoi noise
float2 Rand2D(float xPos, float yPos)
{
    float2 smallValue = float2(sin(xPos), sin(yPos));

    float2 dotDir = float2(12.989f, 78.233f);
    float random = dot(smallValue, dotDir);

    float x = sin(random) * 143758.5453f;
    x -= floor(x);

    dotDir = float2(39.346f, 11.135f);
    random = dot(smallValue, dotDir);

    float y = sin(random) * 143758.5453f;
    y -= floor(y);

    return float2(x, y);
}

float VoronoiNoise(float xPos, float yPos, float cellSize, int type)
{
    float xcoord = abs(xPos);
    float ycoord = abs(yPos);

    float xPosition = xcoord / cellSize;
    float yPosition = ycoord / cellSize;

    uint xBase = floor(xPosition);
    uint yBase = floor(yPosition);

    float minDistance = 100;
    float minDistance2 = 110;
    float minDistance3 = 120;

    for(int x=-1; x<=1; x++) {
        for(int y=-1; y<=1; y++) {
            int cellX = xBase + x;
            int cellY = yBase + y;

            //don't try to read outside the bounds of the array
            if(cellX < 0) {
                continue;
            }
            if(cellY < 0) {
                continue;
            }

            float2 voronoiPos = float2(cellX, cellY) + Rand2D(cellX, cellY);
            float distToCell = sqrt((xPosition - voronoiPos.x) * (xPosition - voronoiPos.x) + (yPosition - voronoiPos.y) * (yPosition - voronoiPos.y));
            //Vector2.Distance(voronoiCells[cell.x, cell.y], pos);

            if(distToCell < minDistance){
                minDistance3 = minDistance2;
                minDistance2 = minDistance;
                minDistance = distToCell;
            } else if(distToCell < minDistance2) {
                minDistance3 = minDistance2;
                minDistance2 = distToCell;
            } else if(distToCell < minDistance3) {
                minDistance3 = distToCell;
            }
        }
    }

    float result = 0;

    if(type == 0) {          //F1
        result = minDistance;
    } else if(type == 1) {   //F2
        result = minDistance2;
    } else if(type == 2) {   //F2 - F1
        result = minDistance2 - minDistance;
    } else if(type == 3) {   //F3
        result = minDistance3;
    } else if(type == 4) {   //F3 - F1
        result = minDistance3 - minDistance;
    } else if(type == 5) {   //F3 - F2
        result = minDistance3 - minDistance2;
    }

    return result;
}

#endif