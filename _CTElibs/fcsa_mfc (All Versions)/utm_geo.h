#ifndef __UTM_GEO_H_INCLUDED
#define __UTM_GEO_H_INCLUDED



enum { NAD_83_DATUM, NAD_27_DATUM } ;

typedef struct
{ double latitude ;  /* positive north in degrees        */
  double longitude ; /* positive west in degrees         */
  double easting ;   /* kilometers                       */
  double northing ;  /* kilometers                       */
  short zone ;       /* utm zone number                  */
  short datum ;      /* NAD_83_DATUM or NAD_27_DATUM     */
} geo_utm_xfer ;

void LatLng_to_UTM(geo_utm_xfer *gxfer) ;
void UTM_to_LatLng(geo_utm_xfer *gxfer) ;

#endif
