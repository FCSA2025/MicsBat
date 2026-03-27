#ifndef __GEN_PROF_H_INCLUDED
#define __GEN_PROF_H_INCLUDED

enum { PRF_OK, PRF_NOMAP, PRF_READERR, PRF_NOMEM, PRF_BADHEADER, PRF_NODATA } ;

typedef struct
{ /* input variables  */
  double lat_1 ;          /* 1st site latitude - positive north - NAD 83 */
  double lng_1 ;          /* 1st site longitude - positive west - NAD 83 */
  double lat_2 ;          /* 2nd site latitude - positive north - NAD 83 */
  double lng_2 ;          /* 2nd site longitude - positive west - NAD 83 */
  double dist_inc ;       /* distance between points - meters */
//  char cded50k_directory[MAX_PATH] ;  // new 1:50,000 CDED data directory
//  char cded250k_directory[MAX_PATH] ; // old 1:250,000 CDED data directory

  /* output profile */
  int num_points ;     /* number of points in the distance-elevation arrays*/
  int error_code ;
  double *dist_array ; /* distance array*/
  double *elev_array ; /* elevation array*/
  double last_dist ;   /* the path length-distance of the last point read*/
			                 /* in the case of an error */
  char map_name[MAX_PATH] ;  /* the DTED file name on which the error occurred*/
  char cded50_map_name[MAX_PATH] ;
  bool cded50_files_used ;

} profile_xfer ;

void create_path_profile(profile_xfer *p_x) ;

// call this function once at the start of the program
// char *dir50k  new 1:50,000 CDED data directory
// char *dir250k  old 1:250,000 CDED data directory
void init_directories(char *dir250k, char *dir50k) ;

#endif
