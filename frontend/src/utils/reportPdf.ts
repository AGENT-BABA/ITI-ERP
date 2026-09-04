import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import type { TradeAttendanceReport, StudentAttendanceReport, ProgressiveAttendanceReport, ProgressCard } from '../api/report.api';

const MONTHS = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

function addHeader(doc: jsPDF, title: string, subtitle?: string) {
  doc.setFontSize(18);
  doc.setFont('helvetica', 'bold');
  doc.text(title, 14, 20);
  if (subtitle) {
    doc.setFontSize(11);
    doc.setFont('helvetica', 'normal');
    doc.setTextColor(100, 100, 100);
    doc.text(subtitle, 14, 28);
    doc.setTextColor(0, 0, 0);
  }
  doc.setDrawColor(200, 200, 200);
  doc.line(14, subtitle ? 32 : 25, 196, subtitle ? 32 : 25);
  return subtitle ? 38 : 30;
}

export function downloadTradeReportPdf(report: TradeAttendanceReport, month: number, year: number) {
  const doc = new jsPDF('landscape');
  const monthName = MONTHS[month - 1];
  const title = `Trade Attendance Report — ${report.tradeCode || ''} ${report.tradeName || ''}`;
  const subtitle = `${monthName} ${year}`;
  let y = addHeader(doc, title, subtitle);

  doc.setFontSize(11);
  doc.setFont('helvetica', 'bold');
  doc.text('Summary', 14, y);
  y += 6;

  autoTable(doc, {
    startY: y,
    head: [['Working Days', 'Students', 'Avg Attendance', 'Above Threshold', 'Below Threshold']],
    body: [[
      report.totalWorkingDays.toString(),
      report.summary.totalStudents.toString(),
      `${report.summary.averageAttendance}%`,
      report.summary.studentsAboveThreshold.toString(),
      report.summary.studentsBelowThreshold.toString(),
    ]],
    theme: 'grid',
    headStyles: { fillColor: [37, 99, 235] },
    styles: { fontSize: 9 },
    margin: { left: 14 },
  });

  y = (doc as any).lastAutoTable.finalY + 10;
  doc.setFontSize(11);
  doc.setFont('helvetica', 'bold');
  doc.text('Student Attendance Details', 14, y);
  y += 2;

  autoTable(doc, {
    startY: y,
    head: [['Roll No', 'Student Name', 'Present', 'Absent', 'Late', 'CL', 'EL', 'ML', 'HO', 'Att %']],
    body: report.students.map((s) => [
      s.rollNumber || '',
      s.studentName || '',
      s.presentDays.toString(),
      s.absentDays.toString(),
      s.lateDays.toString(),
      s.clDays.toString(),
      s.elDays.toString(),
      s.mlDays.toString(),
      s.hoDays.toString(),
      `${s.attendancePercentage}%`,
    ]),
    theme: 'grid',
    headStyles: { fillColor: [37, 99, 235] },
    styles: { fontSize: 8 },
    margin: { left: 14 },
  });

  doc.save(`TradeReport_${report.tradeCode || 'trade'}_${monthName}_${year}.pdf`);
}

export function downloadStudentReportPdf(report: StudentAttendanceReport, month: number, year: number) {
  const doc = new jsPDF();
  const monthName = MONTHS[month - 1];
  const title = 'Student Attendance Report';
  const subtitle = `${report.studentName || ''} (${report.rollNumber || ''}) — ${monthName} ${year}`;
  let y = addHeader(doc, title, subtitle);

  doc.setFontSize(10);
  doc.setFont('helvetica', 'normal');
  doc.text(`Trade: ${report.tradeCode || ''} - ${report.tradeName || ''}`, 14, y);
  y += 8;

  doc.setFontSize(11);
  doc.setFont('helvetica', 'bold');
  doc.text('Summary', 14, y);
  y += 6;

  autoTable(doc, {
    startY: y,
    head: [['Working Days', 'Present', 'Absent', 'Late', 'CL', 'EL', 'ML', 'HO', 'Attendance']],
    body: [[
      report.totalWorkingDays.toString(),
      report.presentDays.toString(),
      report.absentDays.toString(),
      report.lateDays.toString(),
      report.clDays.toString(),
      report.elDays.toString(),
      report.mlDays.toString(),
      report.hoDays.toString(),
      `${report.attendancePercentage}%`,
    ]],
    theme: 'grid',
    headStyles: { fillColor: [37, 99, 235] },
    styles: { fontSize: 9 },
    margin: { left: 14 },
  });

  y = (doc as any).lastAutoTable.finalY + 10;
  doc.setFontSize(11);
  doc.setFont('helvetica', 'bold');
  doc.text('Daily Records', 14, y);
  y += 2;

  autoTable(doc, {
    startY: y,
    head: [['Date', 'Status', 'Remarks']],
    body: report.dailyRecords.map((r) => [
      new Date(r.date).toLocaleDateString(),
      r.status,
      r.remarks || '-',
    ]),
    theme: 'grid',
    headStyles: { fillColor: [37, 99, 235] },
    styles: { fontSize: 8 },
    margin: { left: 14 },
  });

  doc.save(`StudentReport_${report.rollNumber || 'student'}_${monthName}_${year}.pdf`);
}

export function downloadProgressiveReportPdf(report: ProgressiveAttendanceReport) {
  const doc = new jsPDF('landscape');
  const title = 'Progressive Attendance Report';
  const subtitle = `${report.studentName || ''} (${report.rollNumber || ''}) — ${report.tradeCode || ''} ${report.tradeName || ''}`;
  let y = addHeader(doc, title, subtitle);

  doc.setFontSize(10);
  doc.setFont('helvetica', 'normal');
  doc.text(`Session: ${report.sessionYear || ''} | Started: ${new Date(report.sessionStartDate).toLocaleDateString()}`, 14, y);
  y += 8;

  doc.setFontSize(11);
  doc.setFont('helvetica', 'bold');
  doc.text('Cumulative Summary', 14, y);
  y += 6;

  autoTable(doc, {
    startY: y,
    head: [['Working Days', 'Present', 'Absent', 'Late', 'CL', 'EL', 'ML', 'HO', 'Attendance']],
    body: [[
      report.cumulative.totalWorkingDays.toString(),
      report.cumulative.totalPresent.toString(),
      report.cumulative.totalAbsent.toString(),
      report.cumulative.totalLate.toString(),
      report.cumulative.totalCL.toString(),
      report.cumulative.totalEL.toString(),
      report.cumulative.totalML.toString(),
      report.cumulative.totalHO.toString(),
      `${report.cumulative.cumulativePercentage}%`,
    ]],
    theme: 'grid',
    headStyles: { fillColor: [37, 99, 235] },
    styles: { fontSize: 9 },
    margin: { left: 14 },
  });

  y = (doc as any).lastAutoTable.finalY + 10;
  doc.setFontSize(11);
  doc.setFont('helvetica', 'bold');
  doc.text('Month-by-Month Breakdown', 14, y);
  y += 2;

  autoTable(doc, {
    startY: y,
    head: [['Month', 'Working Days', 'Present', 'Absent', 'Late', 'CL', 'EL', 'ML', 'HO', 'Att %']],
    body: report.months.map((m) => [
      m.monthName,
      m.workingDays.toString(),
      m.present.toString(),
      m.absent.toString(),
      m.late.toString(),
      m.cl.toString(),
      m.el.toString(),
      m.ml.toString(),
      m.ho.toString(),
      `${m.attendancePercentage}%`,
    ]),
    theme: 'grid',
    headStyles: { fillColor: [37, 99, 235] },
    styles: { fontSize: 8 },
    margin: { left: 14 },
  });

  doc.save(`ProgressiveReport_${report.rollNumber || 'student'}.pdf`);
}

export function downloadProgressCardPdf(report: ProgressCard) {
  const doc = new jsPDF('landscape');
  let y = 15;

  doc.setFontSize(14);
  doc.setFont('helvetica', 'bold');
  doc.text('Government of India', 148, y, { align: 'center' });
  y += 5;
  doc.setFontSize(9);
  doc.setFont('helvetica', 'normal');
  doc.text('Ministry of Skill Development And Entrepreneurship', 148, y, { align: 'center' });
  y += 6;
  doc.setFontSize(12);
  doc.setFont('helvetica', 'bold');
  doc.text(report.instituteName || 'Institute', 148, y, { align: 'center' });
  y += 5;
  doc.setFontSize(9);
  doc.setFont('helvetica', 'normal');
  doc.text(report.instituteAddress || '', 148, y, { align: 'center' });
  y += 5;
  doc.text(`NSQF Level: ${report.yearLevel}`, 148, y, { align: 'center' });
  y += 3;
  doc.setDrawColor(0);
  doc.line(14, y, 282, y);
  y += 5;

  doc.setFontSize(9);
  doc.setFont('helvetica', 'bold');
  doc.text(`Trainee: ${report.studentName || ''}  |  Roll: ${report.rollNumber || ''}  |  Trade: ${report.tradeName || ''} (${report.tradeCode || ''})  |  Year Level: ${report.yearLevelLabel || ''}`, 14, y);
  y += 4;
  doc.text(`Father: ${report.fatherName || ''}  |  Mother: ${report.motherName || ''}  |  DOB: ${report.dateOfBirth ? new Date(report.dateOfBirth).toLocaleDateString() : ''}  |  Admission: ${report.admissionNumber || ''}`, 14, y);
  y += 8;

  doc.setFontSize(11);
  doc.setFont('helvetica', 'bold');
  doc.text('Monthly Test Marks', 14, y);
  y += 2;

  autoTable(doc, {
    startY: y,
    head: [['Month', 'PR/300', 'TT/100', 'ES/50', 'RECAL/50', 'DRG/50', 'Total/550']],
    body: report.monthlyMarks.map((m) => [
      m.monthName || '',
      m.practicalMarksScaled.toString(),
      '—',
      '—',
      '—',
      '—',
      m.total.toString(),
    ]),
    theme: 'grid',
    headStyles: { fillColor: [37, 99, 235], fontSize: 8 },
    styles: { fontSize: 8 },
    margin: { left: 14 },
  });

  y = (doc as any).lastAutoTable.finalY + 8;
  doc.setFontSize(11);
  doc.setFont('helvetica', 'bold');
  doc.text('Quarterly Assessment', 14, y);
  y += 2;

  autoTable(doc, {
    startY: y,
    head: [['Quarter', 'Possible', 'Working', '%', 'PR/T', 'TT', 'W.CAL', 'DRG', 'Total/150']],
    body: report.quarterlyAssessments.map((q) => [
      `Q${q.quarter}`,
      q.possibleDays.toString(),
      q.workingDays.toString(),
      `${q.attendancePercentage}%`,
      '—',
      '—',
      '—',
      '—',
      q.sessionalTotal.toString(),
    ]),
    theme: 'grid',
    headStyles: { fillColor: [37, 99, 235], fontSize: 8 },
    styles: { fontSize: 8 },
    margin: { left: 14 },
  });

  doc.save(`ProgressCard_${report.rollNumber || 'student'}.pdf`);
}
