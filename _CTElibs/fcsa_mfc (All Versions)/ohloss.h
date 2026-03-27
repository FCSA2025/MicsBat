#ifndef __OHLOSS_H_INCLUDED
#define __OHLOSS_H_INCLUDED


enum { OHL_LOS, OHL_SKE, OHL_ISOL, OHL_DKE, OHL_IRT } ;

typedef struct
{
  /* input parameters*/
  double lat_1 ;          /* 1st site latitude - positive north - NAD 83*/
  double lng_1 ;          /* 1st site longitude - positive west - NAD 83*/
  double lat_2 ;          /* 2nd site latitude - positive north - NAD 83*/
  double lng_2 ;          /* 2nd site longitude - positive west - NAD 83*/
  double s1_anthght ;     /* 1st site antenna height - meters AGL*/
  double s2_anthght ;     /* 2nd site antenna height - meters AGL*/
  double freq ;           /* frequency - MHz say 2000.*/
  short polarization ;    /* 0 - horizontal, 1 - vertical*/
  short clim_region  ;    /* 0 - continental temperate
                             1 - maritime temperate overland
                             2 - maritime temperate oversea
                          */
  double K_median ;       /* median value of K - close to 4/3  say 1.3333*/

  /* output parameters*/
  double dist_horiz1 ;    /* 1st site horizon distance (km)*/
  double dist_horiz2 ;    /* 2nd site horizon distance (km)*/
  double elev_horiz1 ;    /* 1st site horizon elevation (m)*/
  double elev_horiz2 ;    /* 2nd site horizon elevation (m)*/
  double angl_horiz1 ;    /* 1st site horizon angle (deg)*/
  double angl_horiz2 ;    /* 2nd site horizon angle (deg)*/
  double s1_effhght ;     /* effective antenna heights*/
  double s2_effhght ;
  double eff_dist ;       /* effective distance (km)*/
  double horiz_xover ;    /* intersection of horizon rays (km)*/

  double refdiff_loss ;   /* reference diffraction loss (dB)*/
  double refscatt_loss ;  /* reference scatter loss (dB)*/
  double refcomb_loss ;   /* reference combined loss (dB)*/
  double median_loss ;    /* median loss L(0.5) (dB)*/
  char path_record[256] ; /* Description of the Path */
  double ohloss_50[8] ;   /* OHLOSS array for 50 & 95% confidence factors*/
  double ohloss_95[8] ;    /* corresponding to the following time percentages
                              50.0, 80.0, 90.0, 99.0, 99.90,
                              99.990, 99.9950, 99.9975*/
                          /* classification of the path*/
  int calc_type ;         /* OHL_LOS, OHL_SKE, OHL_ISOL, OHL_DKE, OHL_IRT */

  int error_status ;
  char map_name[80] ;
  char cded50_map_name[MAX_PATH] ;
  bool cded50_files_used ;
} ohloss_xfer ;

void calc_ohloss(ohloss_xfer *o_x) ;

#endif

