import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  LinearProgress,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
  Chip,
} from '@mui/material';
import {
  People as PeopleIcon,
  School as SchoolIcon,
  TrendingUp as TrendingUpIcon,
  Assessment as AssessmentIcon,
  CheckCircle as CheckCircleIcon,
  Cancel as CancelIcon,
  Timeline as TimelineIcon,
  Work as WorkIcon,
} from '@mui/icons-material';
import { getDashboardSummary } from '../../api/dashboard.api';

const STATUS_COLORS: Record<string, 'success' | 'error' | 'warning' | 'info' | 'default'> = {
  Active: 'success',
  Inactive: 'warning',
  Completed: 'info',
  Archived: 'default',
  Transferred: 'info',
  DroppedOut: 'error',
  CancelledAdmission: 'error',
};

export default function DashboardPage() {
  const { data: summary, isLoading } = useQuery({
    queryKey: ['dashboardSummary'],
    queryFn: getDashboardSummary,
  });

  if (isLoading) {
    return (
      <Box>
        <Typography variant="h4" gutterBottom sx={{ fontWeight: 600 }}>
          Dashboard
        </Typography>
        <LinearProgress />
      </Box>
    );
  }

  const summaryCards = [
    { title: 'Total Students', value: summary?.totalStudents ?? 0, icon: <PeopleIcon />, color: '#1976d2' },
    { title: 'Active Students', value: summary?.activeStudents ?? 0, icon: <SchoolIcon />, color: '#2e7d32' },
    { title: 'Total Trades', value: summary?.totalTrades ?? 0, icon: <WorkIcon />, color: '#f57c00' },
    { title: 'Total Users', value: summary?.totalUsers ?? 0, icon: <PeopleIcon />, color: '#7b1fa2' },
    { title: 'Attendance %', value: `${summary?.overallAttendancePercentage ?? 0}%`, icon: <TrendingUpIcon />, color: '#00838f' },
    { title: 'Monthly Practical Pass %', value: `${summary?.monthlyPracticalPassPercentage ?? 0}%`, icon: <AssessmentIcon />, color: '#558b2f' },
  ];

  return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ fontWeight: 600 }}>
        Dashboard
      </Typography>

      <Grid container spacing={3}>
        {summaryCards.map((card) => (
          <Grid size={{ xs: 12, sm: 6, md: 4 }} key={card.title}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Box sx={{ color: card.color }}>{card.icon}</Box>
                  <Box>
                    <Typography variant="h4" color={card.color} sx={{ fontWeight: 600 }}>
                      {card.value}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {card.title}
                    </Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={3} sx={{ mt: 2 }}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Trade Seat Occupancy
              </Typography>
              <List dense>
                {summary?.tradeSeatOccupancy.slice(0, 8).map((trade) => (
                  <ListItem key={trade.tradeId}>
                    <ListItemIcon>
                      <WorkIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText
                      primary={`${trade.tradeCode} - ${trade.tradeName}`}
                      secondary={`${trade.occupiedSeats}/${trade.totalSeats} seats`}
                    />
                    <Chip
                      label={`${trade.occupancyPercentage}%`}
                      color={trade.occupancyPercentage >= 80 ? 'success' : trade.occupancyPercentage >= 50 ? 'warning' : 'error'}
                      size="small"
                    />
                  </ListItem>
                ))}
                {(!summary?.tradeSeatOccupancy || summary.tradeSeatOccupancy.length === 0) && (
                  <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
                    No trade data available
                  </Typography>
                )}
              </List>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Recent Activities
              </Typography>
              <List dense>
                {summary?.recentActivities.slice(0, 8).map((activity, index) => (
                  <Box key={index}>
                    <ListItem>
                      <ListItemIcon>
                        {activity.action === 'Create' ? (
                          <CheckCircleIcon fontSize="small" color="success" />
                        ) : activity.action === 'Update' ? (
                          <TimelineIcon fontSize="small" color="info" />
                        ) : (
                          <CancelIcon fontSize="small" color="error" />
                        )}
                      </ListItemIcon>
                      <ListItemText
                        primary={`${activity.action} ${activity.entityName}`}
                        secondary={`${activity.userName || 'System'} - ${new Date(activity.timestamp).toLocaleString()}`}
                      />
                    </ListItem>
                    {index < (summary?.recentActivities.length ?? 0) - 1 && <Divider />}
                  </Box>
                ))}
                {(!summary?.recentActivities || summary.recentActivities.length === 0) && (
                  <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
                    No recent activities
                  </Typography>
                )}
              </List>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Student Status Distribution
              </Typography>
              <List dense>
                {summary?.studentStatusDistribution.map((status) => (
                  <ListItem key={status.status}>
                    <ListItemText
                      primary={status.status}
                      secondary={`${status.count} students (${status.percentage}%)`}
                    />
                    <Chip
                      label={status.status}
                      color={STATUS_COLORS[status.status] || 'default'}
                      size="small"
                    />
                  </ListItem>
                ))}
                {(!summary?.studentStatusDistribution || summary.studentStatusDistribution.length === 0) && (
                  <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
                    No student data available
                  </Typography>
                )}
              </List>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Quick Stats
              </Typography>
              <List dense>
                <ListItem>
                  <ListItemText primary="Academic Sessions" secondary={`${summary?.activeAcademicSessions ?? 0} active of ${summary?.totalAcademicSessions ?? 0} total`} />
                </ListItem>
                <Divider />
                <ListItem>
                  <ListItemText primary="Monthly Practicals" secondary={`${summary?.totalMonthlyPracticals ?? 0} conducted`} />
                </ListItem>
                <Divider />
                <ListItem>
                  <ListItemText primary="Yearly Practicals" secondary={`${summary?.totalYearlyPracticals ?? 0} conducted`} />
                </ListItem>
                <Divider />
                <ListItem>
                  <ListItemText primary="Yearly Practical Pass %" secondary={`${summary?.yearlyPracticalPassPercentage ?? 0}%`} />
                </ListItem>
              </List>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
