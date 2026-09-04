import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { useTheme } from '@mui/material/styles';
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
  Divider,
  Chip,
} from '@mui/material';
import {
  People as PeopleIcon,
  TrendingUp as TrendingUpIcon,
  Fingerprint as AttendanceIcon,
  CalendarMonth as CalendarMonthIcon,
  School as SchoolIcon,
  EventAvailable as EventAvailableIcon,
} from '@mui/icons-material';
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  ResponsiveContainer,
  BarChart,
  Bar,
  Legend,
} from 'recharts';
import { getDashboardSummary } from '../../api/dashboard.api';
import { useAuth } from '../../hooks/useAuth';
import type { DashboardSummary, AttendanceTrend, TradeSeatOccupancy } from '../../api/dashboard.api';

const ICON_BG_LIGHT = 'rgba(0,0,0,0.04)';
const ICON_BG_DARK = 'rgba(255,255,255,0.08)';

function StatCard({ title, value, icon, color }: { title: string; value: string | number; icon: React.ReactNode; color: string }) {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';

  return (
    <Card>
      <CardContent sx={{ py: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box
            sx={{
              color: `${color}.main`,
              bgcolor: isDark ? ICON_BG_DARK : ICON_BG_LIGHT,
              p: 1,
              borderRadius: 1.5,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            {icon}
          </Box>
          <Box>
            <Typography variant="h5" sx={{ fontWeight: 700, lineHeight: 1.2 }}>
              {value}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ fontSize: 13 }}>
              {title}
            </Typography>
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
}

function AttendanceTrendChart({ data }: { data: AttendanceTrend[] }) {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';

  const gridColor = isDark ? 'rgba(255,255,255,0.08)' : '#E2E8F0';
  const axisColor = isDark ? 'rgba(255,255,255,0.45)' : '#94A3B8';
  const lineColor = '#60A5FA';
  const tooltipBg = isDark ? 'rgba(15,23,42,0.9)' : '#fff';
  const tooltipBorder = isDark ? 'rgba(255,255,255,0.15)' : '#E2E8F0';
  const tooltipText = isDark ? '#F8FAFC' : '#172033';

  if (!data || data.length === 0) {
    return (
      <Box sx={{ py: 3, textAlign: 'center' }}>
        <Typography variant="body2" color="text.secondary">No attendance data available</Typography>
      </Box>
    );
  }

  const gradId = 'attGrad';

  return (
    <ResponsiveContainer width="100%" height={220}>
      <AreaChart data={data.slice(-15)} margin={{ top: 5, right: 5, left: -20, bottom: 0 }}>
        <defs>
          <linearGradient id={gradId} x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor={lineColor} stopOpacity={isDark ? 0.35 : 0.25} />
            <stop offset="95%" stopColor={lineColor} stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid strokeDasharray="3 3" stroke={gridColor} />
        <XAxis
          dataKey="date"
          tickFormatter={(v) => new Date(v).toLocaleDateString('en', { day: 'numeric', month: 'short' })}
          tick={{ fontSize: 11, fill: axisColor }}
          stroke={gridColor}
        />
        <YAxis domain={[0, 100]} tick={{ fontSize: 11, fill: axisColor }} stroke={gridColor} />
        <RechartsTooltip
          formatter={(value: any) => [`${value}%`, 'Attendance']}
          labelFormatter={(label: any) => new Date(label).toLocaleDateString()}
          contentStyle={{
            fontSize: 12,
            background: tooltipBg,
            border: `1px solid ${tooltipBorder}`,
            borderRadius: 8,
            color: tooltipText,
            boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
          }}
          itemStyle={{ color: tooltipText }}
        />
        <Area
          type="monotone"
          dataKey="attendancePercentage"
          stroke={lineColor}
          strokeWidth={2}
          fill={`url(#${gradId})`}
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}

function TradeOccupancyChart({ data }: { data: TradeSeatOccupancy[] }) {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';

  const gridColor = isDark ? 'rgba(255,255,255,0.08)' : '#E2E8F0';
  const axisColor = isDark ? 'rgba(255,255,255,0.45)' : '#94A3B8';
  const occupiedColor = '#60A5FA';
  const totalColor = isDark ? 'rgba(255,255,255,0.15)' : '#CBD5E1';
  const tooltipBg = isDark ? 'rgba(15,23,42,0.9)' : '#fff';
  const tooltipBorder = isDark ? 'rgba(255,255,255,0.15)' : '#E2E8F0';
  const tooltipText = isDark ? '#F8FAFC' : '#172033';

  if (!data || data.length === 0) {
    return (
      <Box sx={{ py: 6, textAlign: 'center' }}>
        <Typography variant="body2" color="text.secondary">No trade data available</Typography>
      </Box>
    );
  }

  return (
    <ResponsiveContainer width="100%" height={220}>
      <BarChart data={data} margin={{ top: 5, right: 5, left: -20, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" stroke={gridColor} />
        <XAxis dataKey="tradeCode" tick={{ fontSize: 11, fill: axisColor }} stroke={gridColor} />
        <YAxis tick={{ fontSize: 11, fill: axisColor }} stroke={gridColor} />
        <RechartsTooltip
          formatter={(value: any, name: any) => [value, name === 'occupiedSeats' ? 'Occupied' : 'Total']}
          contentStyle={{
            fontSize: 12,
            background: tooltipBg,
            border: `1px solid ${tooltipBorder}`,
            borderRadius: 8,
            color: tooltipText,
            boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
          }}
          itemStyle={{ color: tooltipText }}
          cursor={{ fill: isDark ? 'rgba(255,255,255,0.04)' : 'rgba(0,0,0,0.04)' }}
        />
        <Legend
          wrapperStyle={{ fontSize: 12, color: axisColor }}
          formatter={(value) => (value === 'occupiedSeats' ? 'Occupied' : 'Total')}
        />
        <Bar dataKey="occupiedSeats" fill={occupiedColor} radius={[4, 4, 0, 0]} barSize={24} />
        <Bar dataKey="totalSeats" fill={totalColor} radius={[4, 4, 0, 0]} barSize={24} />
      </BarChart>
    </ResponsiveContainer>
  );
}

function TradeHeadDashboard({ summary }: { summary: DashboardSummary }) {
  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 600, mb: 2 }}>
        Dashboard — {summary.tradeName || 'My Trade'}
      </Typography>

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard title="My Students" value={summary.activeStudents ?? 0} icon={<PeopleIcon />} color="primary" />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard title="Monthly Practicals" value={summary.totalMonthlyPracticals ?? 0} icon={<CalendarMonthIcon />} color="warning" />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard title="Yearly Practicals" value={summary.totalYearlyPracticals ?? 0} icon={<SchoolIcon />} color="info" />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard title="Attendance" value={`${summary.overallAttendancePercentage ?? 0}%`} icon={<AttendanceIcon />} color="success" />
        </Grid>
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardContent>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                Attendance Trend
              </Typography>
              <AttendanceTrendChart data={summary.attendanceTrends} />
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Card>
            <CardContent>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                Student Distribution
              </Typography>
              {summary.studentStatusDistribution.length === 0 ? (
                <Typography variant="body2" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
                  No student data available
                </Typography>
              ) : (
                <List dense disablePadding>
                  {summary.studentStatusDistribution.map((status, i) => (
                    <React.Fragment key={status.status}>
                      <ListItem sx={{ px: 0 }}>
                        <ListItemText
                          primary={status.status}
                          secondary={`${status.count} students (${status.percentage}%)`}
                          slotProps={{
                            primary: { variant: 'body2', sx: { fontWeight: 500 } },
                            secondary: { variant: 'caption' },
                          }}
                        />
                        <Chip
                          label={`${status.percentage}%`}
                          size="small"
                          color={status.status === 'Active' ? 'success' : status.status === 'Withdrawn' ? 'error' : 'default'}
                          sx={{ height: 22, fontSize: 11 }}
                        />
                      </ListItem>
                      {i < summary.studentStatusDistribution.length - 1 && <Divider />}
                    </React.Fragment>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}

function AdminDashboard({ summary, isSuperAdmin }: { summary: DashboardSummary; isSuperAdmin?: boolean }) {
  const summaryCards = isSuperAdmin
    ? [
        { title: 'Total Institutes', value: summary.totalInstitutes ?? 0, icon: <SchoolIcon />, color: 'primary' },
        { title: 'Total Users', value: summary.totalUsers ?? 0, icon: <PeopleIcon />, color: 'info' },
        { title: 'Academic Sessions', value: summary.totalAcademicSessions ?? 0, icon: <CalendarMonthIcon />, color: 'warning' },
        { title: 'Active Sessions', value: summary.activeAcademicSessions ?? 0, icon: <EventAvailableIcon />, color: 'success' },
      ]
    : [
        { title: 'Total Students', value: summary.totalStudents ?? 0, icon: <PeopleIcon />, color: 'primary' },
        { title: 'Active Students', value: summary.activeStudents ?? 0, icon: <PeopleIcon />, color: 'success' },
        { title: 'Total Trades', value: summary.totalTrades ?? 0, icon: <TrendingUpIcon />, color: 'info' },
        { title: 'Attendance Rate', value: `${summary.overallAttendancePercentage ?? 0}%`, icon: <AttendanceIcon />, color: 'warning' },
      ];

  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 600, mb: 2 }}>
        Dashboard
      </Typography>

      <Grid container spacing={2} sx={{ mb: 3 }}>
        {summaryCards.map((card) => (
          <Grid size={{ xs: 6, md: 3 }} key={card.title}>
            <StatCard title={card.title} value={card.value} icon={card.icon} color={card.color} />
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardContent sx={{ minHeight: 280 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                Attendance Trend
              </Typography>
              <AttendanceTrendChart data={summary.attendanceTrends} />
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Card>
            <CardContent sx={{ minHeight: 280 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                Trade Seat Occupancy
              </Typography>
              <TradeOccupancyChart data={summary.tradeSeatOccupancy} />
            </CardContent>
          </Card>
        </Grid>

        {!isSuperAdmin && (
          <Grid size={{ xs: 12, md: 6 }}>
            <Card>
              <CardContent>
                <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                  Recent Activity
                </Typography>
                {summary.recentActivities.length === 0 ? (
                  <Typography variant="body2" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
                    No recent activity
                  </Typography>
                ) : (
                  <List dense disablePadding>
                    {summary.recentActivities.slice(0, 6).map((activity, i) => (
                      <React.Fragment key={i}>
                        <ListItem sx={{ px: 0 }}>
                          <ListItemText
                            primary={activity.entityName}
                            secondary={`${activity.action}${activity.userName ? ` by ${activity.userName}` : ''} — ${new Date(activity.timestamp).toLocaleDateString()}`}
                            slotProps={{
                              primary: { variant: 'body2', sx: { fontWeight: 500 } },
                              secondary: { variant: 'caption' },
                            }}
                          />
                        </ListItem>
                        {i < Math.min(summary.recentActivities.length, 6) - 1 && <Divider />}
                      </React.Fragment>
                    ))}
                  </List>
                )}
              </CardContent>
            </Card>
          </Grid>
        )}

        <Grid size={{ xs: 12, md: isSuperAdmin ? 12 : 6 }}>
          <Card>
            <CardContent>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                Student Distribution
              </Typography>
              {summary.studentStatusDistribution.length === 0 ? (
                <Typography variant="body2" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
                  No student data available
                </Typography>
              ) : (
                <List dense disablePadding>
                  {summary.studentStatusDistribution.map((status, i) => (
                    <React.Fragment key={status.status}>
                      <ListItem sx={{ px: 0 }}>
                        <ListItemText
                          primary={status.status}
                          secondary={`${status.count} students (${status.percentage}%)`}
                          slotProps={{
                            primary: { variant: 'body2', sx: { fontWeight: 500 } },
                            secondary: { variant: 'caption' },
                          }}
                        />
                        <Chip
                          label={`${status.percentage}%`}
                          size="small"
                          color={status.status === 'Active' ? 'success' : status.status === 'Withdrawn' ? 'error' : 'default'}
                          sx={{ height: 22, fontSize: 11 }}
                        />
                      </ListItem>
                      {i < summary.studentStatusDistribution.length - 1 && <Divider />}
                    </React.Fragment>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}

export default function DashboardPage() {
  const { hasRole, isSuperAdmin } = useAuth();
  const { data: summary, isLoading } = useQuery({
    queryKey: ['dashboardSummary'],
    queryFn: getDashboardSummary,
  });

  if (isLoading) {
    return (
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 600, mb: 2 }}>Dashboard</Typography>
        <LinearProgress />
      </Box>
    );
  }

  if (!summary) return null;

  if (hasRole('TradeHead')) {
    return <TradeHeadDashboard summary={summary} />;
  }

  if (isSuperAdmin) {
    return <AdminDashboard summary={summary} isSuperAdmin />;
  }

  return <AdminDashboard summary={summary} />;
}
