#ifndef __FWD_INV_H_INCLUDED
#define __FWD_INV_H_INCLUDED

void inverse(double *path_d, double *bear_1, double *bear_2,
             double s1_lat, double s1_lng,
             double s2_lat, double s2_lng) ;
void forward(double path_d, double bear_1, double *bear_2,
             double s1_lat, double s1_lng,
             double *s2_lat, double *s2_lng) ;

#endif
