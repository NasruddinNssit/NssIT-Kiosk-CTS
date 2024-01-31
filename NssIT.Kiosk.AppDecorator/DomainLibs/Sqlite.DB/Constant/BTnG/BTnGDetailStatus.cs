using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG
{
    public enum BTnGDetailStatus
    {
        /// <summary>
        /// Status refer to GoPayment name with 'new'; Status Type : In Progress
        /// </summary>
        @new = 0,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final but Kiosk can proceed to w_cancel_request if necessary.
        /// </summary>
        paid = 1,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        paid_fail = 2,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        cancel_by_api = 3,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        cancel_and_refund_by_api = 4,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        payment_gateway_req_fail = 5,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : In Progress But Kiosk & KTMBCTS will not do any work for this state
        /// </summary>
        init = 6,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : In Progress But Kiosk & KTMBCTS will not do any work for this state
        /// </summary>
        paying = 7,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        expired = 8,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        refunded_by_failed_sales = 9,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        refunded_by_api = 10,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        refunded_by_merchant = 11,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        refunded_by_admin = 12,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        manual_refund = 13,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        other = 14,

        /// <summary>
        /// Status name refer to GoPayment; Status Type : Final
        /// </summary>
        record_not_found = 15,

        /// <summary>
        /// Sale has already Canceled or Refunded
        /// </summary>
        SaleAlreadyCancelRefund = 16,

        /// <summary>
        /// Sale has already Canceled
        /// </summary>
        SaleAlreadyCancel = 17,

        /// <summary>
        /// Sale has already Refunded
        /// </summary>
        SaleAlreadyRefund = 18,

        /// <summary>
        /// Unrecognized status introduced by GoPayment; Status Type : Final; This status used to indicate additional status introduced by GoPayment that is not recognized by KTMBCTS
        /// </summary>
        unknown_fail_status = 999,

        /// <summary>
        /// Cancel event detected successfully at Kiosk; Status Type : In Progress
        /// </summary>
        w_cancel_refund_request = 30,

        /// <summary>
        /// Cancel / Refund already done previous by GoPayment ; Status Type : Final
        /// </summary>
        w_cancel_and_refund_by_api_already_done = 31,

        /// <summary>
        /// Sale successful and save by kiosk/machine to KTMBCTS Web API; ack: Acknowledge; Status Type : Final
        /// </summary>
        w_paid_ack = 32,

        /// <summary>
        /// Sale failed and save by kiosk/machine (onspot : in front of customer) to KTMBCTS Web API; Status Type : Final
        /// </summary>
        w_onspot_sale_mach_fail_ack = 33,

        /// <summary>
        /// Error occur when apply BTnG Web API in KTMBCTS Web API; Status Type : In Progress
        /// </summary>
        w_BTnG_site_error = 34,

        /// <summary>
        /// Timeout occur when executing BTnG Web API in KTMBCTS Web API; Status Type : In Progress
        /// </summary>
        w_BTnG_timeout = 35,

        /// <summary>
        /// Error occur when executing in KTMBCTS Web API; Status Type : In Progress
        /// </summary>
        w_web_api_error = 36,

        /// <summary>
        /// Fail to create new transaction; Status Type : Final
        /// </summary>
        w_fail_create_new = 37,

        /// <summary>
        /// Paid is confirm when review (clean-up) transaction in WebAPI; Status Type : Final
        /// </summary>
        w_paid_confirm_on_cleanup = 38,

        /// <summary>
        /// Error occur at machine/kiosk; Status Type : In Progress
        /// </summary>
        m_machine_error = 70,

        /// <summary>
        /// Timeout at machine/kiosk; Status Type : In Progress
        /// </summary>
        m_machine_timeout = 71,

        /// <summary>
        /// Timeout at machine/kiosk; Occur when kiosk detected a Cancel event successfully; Status Type : In Progress
        /// </summary>
        m_machine_cancel_refund_request = 72,

        /// <summary>
        /// The detail status can refer to KTMBCTS DB. This status use to indicate the BTnG transaction has already done at KTMBCTS previously.
        /// </summary>
        m_done_previously_in_webapi = 79,
    }
}
